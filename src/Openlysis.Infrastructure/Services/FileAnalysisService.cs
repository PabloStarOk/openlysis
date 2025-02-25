using System.Collections.Concurrent;

using Microsoft.AspNetCore.Http.Features;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

using Openlysis.Application.Common.Interfaces.Persistence;
using Openlysis.Application.Common.Interfaces.Ports;
using Openlysis.Application.Common.Interfaces.Services;
using Openlysis.Domain.Common.Enums;
using Openlysis.Domain.FileAnalyses;
using Openlysis.Domain.FileAnalyses.Entities;
using Openlysis.Domain.FileAnalyses.ValueObjects;

namespace Openlysis.Infrastructure.Services;

/// <summary>
/// Service to analyze a file using multi services.
/// </summary>
public class FileAnalysisService : IFileAnalysisService
{
    private readonly IOptions<FormOptions> _formOptions;
    private readonly IServiceScopeFactory _serviceScopeFactory;

    /// <summary>
    /// Initializes a new instance of the <see cref="FileAnalysisService"/> class.
    /// </summary>
    /// <param name="formOptions">The form options for configuring the service.</param>
    /// <param name="serviceScopeFactory">The service scope factory for creating service scopes.</param>
    public FileAnalysisService(
        IOptions<FormOptions> formOptions,
        IServiceScopeFactory serviceScopeFactory)
    {
        _formOptions = formOptions;
        _serviceScopeFactory = serviceScopeFactory;
    }

    /// <inheritdoc/>
    public async Task StartJobAsync(
        FileAnalysisJobRequest request,
        FileMultiAnalysis multiAnalysis,
        CancellationToken cancellationToken)
    {
        // TODO: Use a worker service instead, to capture exceptions and improve scalability, then remove try-catch.
        try
        {
            Stream fileDataStream = await CopyStreamAsync(request.FileStreamData, cancellationToken);
            FileAnalysisJobRequest copiedRequest = request with
            {
                FileStreamData = fileDataStream,
            };
            ThreadPool.QueueUserWorkItem(
                _ => Task.Run(
                    async () =>
                    {
                        try
                        {
                            await using AsyncServiceScope scope = _serviceScopeFactory.CreateAsyncScope();

                            // TODO: Delegate job to an AnalysisJob class which contains analyzers, multiAnalysis, and repository fields.
                            var analyzers = scope.ServiceProvider
                                .GetRequiredService<IEnumerable<IServiceAnalyzer<ServiceFileAnalysis, ServiceFileAnalysisId>>>()
                                .ToArray();
                            await AnalyzeAsync(copiedRequest, analyzers, multiAnalysis, cancellationToken);
                            await UpdateAnalysisAsync(analyzers, multiAnalysis, cancellationToken);

                            var repository = scope.ServiceProvider
                                .GetRequiredService<IRepository<FileMultiAnalysis, FileMultiAnalysisId>>();
                            await SaveAnalysisAsync(repository, multiAnalysis, cancellationToken);
                        }
                        catch (Exception ex)
                        {
                            // TODO: Log exceptions
                        }
                    },
                    cancellationToken));
        }
        catch (OperationCanceledException)
        {
        }
    }

    /// <summary>
    /// Analyzes the file using the available analyzers.
    /// </summary>
    /// <param name="request">The request containing the file analysis details.</param>
    /// <param name="analyzers">The array of analyzers to use for the analysis.</param>
    /// <param name="multiAnalysis">The multi-analysis object to be updated.</param>
    /// <param name="cancellationToken">The cancellation token to cancel the operation.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    private static async Task AnalyzeAsync(
        FileAnalysisJobRequest request,
        IServiceAnalyzer<ServiceFileAnalysis, ServiceFileAnalysisId>[] analyzers,
        FileMultiAnalysis multiAnalysis,
        CancellationToken cancellationToken)
    {
        foreach (var analyzer in analyzers)
        {
            var analyzeResult = await analyzer.AnalyzeAsync(request, cancellationToken);
            if (analyzeResult.IsError)
            {
                // TODO: Log error.
                continue;
            }

            var analysisResult = await analyzer.GetAnalysisAsync(analyzeResult.Value, cancellationToken);
            if (analysisResult.IsError)
            {
                // TODO: Log error.
                continue;
            }

            // Add the analysis result to the list of analyses
            multiAnalysis.AddServiceAnalysis(analysisResult.Value);
        }
    }

    /// <summary>
    /// Updates the analysis asynchronously.
    /// </summary>
    /// <param name="analyzers">The array of analyzers to use for the analysis.</param>
    /// <param name="multiAnalysis">The multi-analysis object to be updated.</param>
    /// <param name="cancellationToken">The cancellation token to cancel the operation.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    private static async Task UpdateAnalysisAsync(
        IServiceAnalyzer<ServiceFileAnalysis, ServiceFileAnalysisId>[] analyzers,
        FileMultiAnalysis multiAnalysis,
        CancellationToken cancellationToken)
    {
        const int updateFrequencyMs = 500;
        ConcurrentBag<ServiceFileAnalysis> analyses = [];
        var analyzersMap = analyzers.ToDictionary(a => a.ServiceName);

        while (!cancellationToken.IsCancellationRequested &&
               (multiAnalysis.Status is not AnalysisStatus.Finished and not AnalysisStatus.Timeout))
        {
            await Parallel.ForEachAsync(
                multiAnalysis.ServiceFileAnalyses, cancellationToken, async (analysis, token) =>
                {
                    var analyzer = analyzersMap[analysis.ServiceName];
                    var result = await analyzer.GetAnalysisAsync(analysis.Id, token);

                    if (result.IsError)
                    {
                        // TODO: Log error.
                        return;
                    }

                    analyses.Add(result.Value);
                });

            while (analyses.TryTake(out ServiceFileAnalysis? analysis))
            {
                multiAnalysis.UpdateServiceAnalysis(analysis);
            }

            await Task.Delay(updateFrequencyMs, cancellationToken); // TODO: Use options instead for update frequency.
        }
    }

    /// <summary>
    /// Saves the analysis asynchronously.
    /// </summary>
    /// <param name="repository">The repository to save the analysis to.</param>
    /// <param name="multiAnalysis">The multi-analysis object to be saved.</param>
    /// <param name="cancellationToken">The cancellation token to cancel the operation.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    private static async Task SaveAnalysisAsync(
        IRepository<FileMultiAnalysis, FileMultiAnalysisId> repository,
        FileMultiAnalysis multiAnalysis,
        CancellationToken cancellationToken)
    {
        // TODO: Response should be cached to avoid doing various queries to the DB.
        if (await repository.ExistsAsync(multiAnalysis.Id, cancellationToken))
        {
            await repository.UpdateAsync(multiAnalysis, cancellationToken);
            return;
        }

        await repository.AddAsync(multiAnalysis, cancellationToken);
    }

    /// <summary>
    /// Copies the provided stream to a new stream.
    /// </summary>
    /// <param name="stream">The stream to be copied.</param>
    /// <param name="cancellationToken">The cancellation token to cancel the operation.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the new stream.</returns>
    private async Task<Stream> CopyStreamAsync(Stream stream, CancellationToken cancellationToken)
    {
        int length = (int)stream.Length;
        Stream newStream;
        if (stream.Length <= _formOptions.Value.MemoryBufferThreshold)
        {
            newStream = new MemoryStream(length);
        }
        else
        {
            string tempFileFullPath = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
            while (File.Exists(tempFileFullPath))
            {
                tempFileFullPath = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
            }

            var options = new FileStreamOptions
            {
                Access = FileAccess.ReadWrite,
                Mode = FileMode.CreateNew,
                Options = FileOptions.Asynchronous | FileOptions.DeleteOnClose,
                Share = FileShare.None,
                PreallocationSize = length,
            };

            newStream = new FileStream(tempFileFullPath, options);
        }

        stream.Position = 0;
        await stream.CopyToAsync(newStream, cancellationToken);
        newStream.Position = 0;
        return newStream;
    }
}
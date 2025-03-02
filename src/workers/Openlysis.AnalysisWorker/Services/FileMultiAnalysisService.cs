using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

using MassTransit;

using Microsoft.Extensions.Options;

using Openlysis.AnalysisWorker.Configuration;
using Openlysis.AnalysisWorker.Consumers.AnalyzeFile;
using Openlysis.Application.Common.Interfaces.Persistence;
using Openlysis.Application.Common.Interfaces.Services;
using Openlysis.Domain.FileAnalyses;
using Openlysis.Domain.FileAnalyses.ValueObjects;

namespace Openlysis.AnalysisWorker.Services;

/// <summary>
/// Service to analyze a file using multi services.
/// </summary>
public class FileMultiAnalysisService : IFileMultiAnalysisService
{
    private readonly IOptions<BrokerSettings> _brokerOptions;
    private readonly IRepository<FileMultiAnalysis, FileMultiAnalysisId> _multiAnalysisRepository;
    private readonly ISendEndpointProvider _sendEndpointProvider;
    private readonly Uri _analyzeFileEndpointUri;

    /// <summary>
    /// Initializes a new instance of the <see cref="FileMultiAnalysisService"/> class.
    /// </summary>
    /// <param name="brokerOptions">The broker settings options.</param>
    /// <param name="sendEndpointProvider">The endpoint to send messages to.</param>
    /// <param name="multiAnalysisRepository">The repository for managing file multi-analysis entities.</param>
    public FileMultiAnalysisService(
        IOptions<BrokerSettings> brokerOptions,
        ISendEndpointProvider sendEndpointProvider,
        IRepository<FileMultiAnalysis, FileMultiAnalysisId> multiAnalysisRepository)
    {
        _brokerOptions = brokerOptions;
        _sendEndpointProvider = sendEndpointProvider;
        this._multiAnalysisRepository = multiAnalysisRepository;

        var uriBuilder = new UriBuilder
        {
            Scheme = "rabbitmq",
            Host = _brokerOptions.Value.Host,
            Port = _brokerOptions.Value.Port,
            Path = AnalyzeFileConsumer.EndpointName,
        };
        _analyzeFileEndpointUri = uriBuilder.Uri;
    }

    /// <inheritdoc/>
    public async Task StartAnalysisAsync(
        FileMultiAnalysis fileMultiAnalysis,
        Stream fileData,
        string fileDescription,
        string filePassword,
        bool isPrivateFile,
        CancellationToken cancellationToken)
    {
        var fileStreamOptions = new FileStreamOptions
        {
            Access = FileAccess.Write,
            Mode = FileMode.CreateNew,
            Options = FileOptions.Asynchronous,
        };

        string tempFilePath = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
        while (File.Exists(tempFilePath))
        {
            tempFilePath = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
        }

        await using (var fileStream = new FileStream(tempFilePath, fileStreamOptions))
        {
            fileData.Position = 0;
            await fileData.CopyToAsync(fileStream, cancellationToken);
        }

        await _multiAnalysisRepository.AddAsync(fileMultiAnalysis, cancellationToken);

        var analyzeFile = new AnalyzeFile(
            fileMultiAnalysis.Id,
            fileMultiAnalysis.FileMetadata.Name,
            fileMultiAnalysis.FileMetadata.ContentType,
            tempFilePath,
            fileDescription,
            filePassword,
            isPrivateFile);

        ISendEndpoint sendEndpoint = await _sendEndpointProvider.GetSendEndpoint(_analyzeFileEndpointUri);
        await sendEndpoint.Send(analyzeFile, cancellationToken);
    }
}
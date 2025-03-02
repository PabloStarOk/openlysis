using System.IO;
using System.Threading;
using System.Threading.Tasks;

using MassTransit;

using Openlysis.AnalysisWorker.Common.Interfaces;
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
    private readonly IEndpointUriProvider _endpointUriProvider;
    private readonly IRepository<FileMultiAnalysis, FileMultiAnalysisId> _multiAnalysisRepository;
    private readonly ISendEndpointProvider _sendEndpointProvider;

    /// <summary>
    /// Initializes a new instance of the <see cref="FileMultiAnalysisService"/> class.
    /// </summary>
    /// <param name="endpointUriProvider">The provider for endpoint URIs.</param>
    /// <param name="sendEndpointProvider">The endpoint to send messages to.</param>
    /// <param name="multiAnalysisRepository">The repository for managing file multi-analysis entities.</param>
    public FileMultiAnalysisService(
        IEndpointUriProvider endpointUriProvider,
        ISendEndpointProvider sendEndpointProvider,
        IRepository<FileMultiAnalysis, FileMultiAnalysisId> multiAnalysisRepository)
    {
        _endpointUriProvider = endpointUriProvider;
        _sendEndpointProvider = sendEndpointProvider;
        this._multiAnalysisRepository = multiAnalysisRepository;
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

        ISendEndpoint sendEndpoint = await _sendEndpointProvider.GetSendEndpoint(_endpointUriProvider.AnalyzeFileUri);
        await sendEndpoint.Send(analyzeFile, cancellationToken);
    }
}
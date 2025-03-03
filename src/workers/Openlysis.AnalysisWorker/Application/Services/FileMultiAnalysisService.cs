using System.IO;
using System.Threading;
using System.Threading.Tasks;

using MassTransit;

using Openlysis.AnalysisWorker.Core.Abstractions;
using Openlysis.AnalysisWorker.Features.AnalyzeFile.Contracts;
using Openlysis.Application.Common.Interfaces.Persistence;
using Openlysis.Application.Common.Interfaces.Services;
using Openlysis.Domain.FileAnalyses;
using Openlysis.Domain.FileAnalyses.ValueObjects;

namespace Openlysis.AnalysisWorker.Application.Services;

/// <summary>
/// Service to analyze a file using multi services.
/// </summary>
public class FileMultiAnalysisService : IFileMultiAnalysisService
{
    private readonly IEndpointUriProvider _endpointUriProvider;
    private readonly IRepository<FileMultiAnalysis, FileMultiAnalysisId> _multiAnalysisRepository;
    private readonly IFileStorageProvider _fileStorageProvider;
    private readonly ISendEndpointProvider _sendEndpointProvider;

    /// <summary>
    /// Initializes a new instance of the <see cref="FileMultiAnalysisService"/> class.
    /// </summary>
    /// <param name="endpointUriProvider">The provider for endpoint URIs.</param>
    /// <param name="sendEndpointProvider">The endpoint to send messages to.</param>
    /// <param name="fileStorageProvider">The provider for file storage operations.</param>
    /// <param name="multiAnalysisRepository">The repository for managing file multi-analysis entities.</param>
    public FileMultiAnalysisService(
        IEndpointUriProvider endpointUriProvider,
        ISendEndpointProvider sendEndpointProvider,
        IFileStorageProvider fileStorageProvider,
        IRepository<FileMultiAnalysis, FileMultiAnalysisId> multiAnalysisRepository)
    {
        _endpointUriProvider = endpointUriProvider;
        _sendEndpointProvider = sendEndpointProvider;
        _fileStorageProvider = fileStorageProvider;
        _multiAnalysisRepository = multiAnalysisRepository;
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
        string fileId = await _fileStorageProvider.UploadAsync(fileData, cancellationToken);
        await _multiAnalysisRepository.AddAsync(fileMultiAnalysis, cancellationToken);

        var analyzeFile = new AnalyzeFile(
            fileMultiAnalysis.Id,
            fileMultiAnalysis.FileMetadata.Name,
            fileMultiAnalysis.FileMetadata.ContentType,
            fileId,
            fileDescription,
            filePassword,
            isPrivateFile);

        ISendEndpoint sendEndpoint = await _sendEndpointProvider.GetSendEndpoint(_endpointUriProvider.AnalyzeFileUri);
        await sendEndpoint.Send(analyzeFile, cancellationToken);
    }
}
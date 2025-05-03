using MassTransit;

using Openlysis.Application.Common.Abstractions.Persistence;
using Openlysis.Application.Files.Contracts.Abstractions;
using Openlysis.Domain.Common.ValueObjects;
using Openlysis.Domain.Files;
using Openlysis.Infrastructure.Shared.Messaging.Abstractions;
using Openlysis.Infrastructure.Shared.Messaging.Models;

namespace Openlysis.Infrastructure.Services.Messaging;

/// <summary>
/// Service to analyze a file using multi services.
/// </summary>
internal class FileMultiAnalyzer : IFileMultiAnalyzer
{
    private readonly IEndpointUriProvider _endpointUriProvider;
    private readonly IRepository<FileMultiAnalysis, GlobalId> _multiAnalysisRepository;
    private readonly IFileStorageProvider _fileStorageProvider;
    private readonly ISendEndpointProvider _sendEndpointProvider;

    /// <summary>
    /// Initializes a new instance of the <see cref="FileMultiAnalyzer"/> class.
    /// </summary>
    /// <param name="endpointUriProvider">The provider for endpoint URIs.</param>
    /// <param name="sendEndpointProvider">The endpoint to send messages to.</param>
    /// <param name="fileStorageProvider">The provider for file storage operations.</param>
    /// <param name="multiAnalysisRepository">The repository for managing file multi-analysis entities.</param>
    public FileMultiAnalyzer(
        IEndpointUriProvider endpointUriProvider,
        ISendEndpointProvider sendEndpointProvider,
        IFileStorageProvider fileStorageProvider,
        IRepository<FileMultiAnalysis, GlobalId> multiAnalysisRepository)
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
            filePassword,
            isPrivateFile);

        ISendEndpoint sendEndpoint = await _sendEndpointProvider.GetSendEndpoint(_endpointUriProvider.AnalyzeFileUri);
        await sendEndpoint.Send(analyzeFile, cancellationToken);
    }
}
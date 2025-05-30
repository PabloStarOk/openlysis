using MassTransit;

using Openlysis.Application.Files.Contracts.Abstractions;
using Openlysis.Domain.Files;
using Openlysis.Infrastructure.Shared.Communication.Abstractions;
using Openlysis.Infrastructure.Shared.Communication.Contracts;

namespace Openlysis.Infrastructure.Services.Files;

/// <summary>
/// Service to analyze a file using multi services.
/// </summary>
internal class FileMultiAnalyzer : IFileMultiAnalyzer
{
    private readonly IEndpointUriProvider _endpointUriProvider;
    private readonly IFileStorageProvider _fileStorageProvider;
    private readonly ISendEndpointProvider _sendEndpointProvider;

    /// <summary>
    /// Initializes a new instance of the <see cref="FileMultiAnalyzer"/> class.
    /// </summary>
    /// <param name="endpointUriProvider">The provider for endpoint URIs.</param>
    /// <param name="sendEndpointProvider">The endpoint to send messages to.</param>
    /// <param name="fileStorageProvider">The provider for file storage operations.</param>
    public FileMultiAnalyzer(
        IEndpointUriProvider endpointUriProvider,
        ISendEndpointProvider sendEndpointProvider,
        IFileStorageProvider fileStorageProvider)
    {
        _endpointUriProvider = endpointUriProvider;
        _sendEndpointProvider = sendEndpointProvider;
        _fileStorageProvider = fileStorageProvider;
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
        var analyzeFile = new AnalyzeFile(
            fileMultiAnalysis.Id,
            fileMultiAnalysis.FileMetadata.Name,
            fileMultiAnalysis.FileMetadata.ContentType,
            fileMultiAnalysis.DataHashValues.Sha256,
            filePassword,
            isPrivateFile,
            fileId);

        ISendEndpoint sendEndpoint = await _sendEndpointProvider.GetSendEndpoint(_endpointUriProvider.AnalyzeFileUri);
        await sendEndpoint.Send(analyzeFile, cancellationToken);
    }
}
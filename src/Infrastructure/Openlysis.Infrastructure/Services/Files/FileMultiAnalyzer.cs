using MassTransit;

using Openlysis.Application.Common.Models;
using Openlysis.Application.Files.Contracts.Abstractions;
using Openlysis.Domain.Common.ValueObjects;
using Openlysis.Infrastructure.Shared.Communication.Abstractions;
using Openlysis.Infrastructure.Shared.Communication.Contracts;

namespace Openlysis.Infrastructure.Services.Files;

/// <summary>
/// Service to analyze a file using multi services.
/// </summary>
internal class FileMultiAnalyzer : IFileMultiAnalyzer
{
    private readonly IEndpointUriProvider _endpointUriProvider;
    private readonly ISendEndpointProvider _sendEndpointProvider;

    /// <summary>
    /// Initializes a new instance of the <see cref="FileMultiAnalyzer"/> class.
    /// </summary>
    /// <param name="endpointUriProvider">The provider for endpoint URIs.</param>
    /// <param name="sendEndpointProvider">The endpoint to send messages to.</param>
    public FileMultiAnalyzer(
        IEndpointUriProvider endpointUriProvider,
        ISendEndpointProvider sendEndpointProvider)
    {
        _endpointUriProvider = endpointUriProvider;
        _sendEndpointProvider = sendEndpointProvider;
    }

    /// <inheritdoc/>
    public async Task StartAnalysisAsync(
        GlobalId multiAnalysisId,
        ProcessedFile processedFile,
        string filePassword,
        bool isPrivateFile,
        CancellationToken cancellationToken)
    {
        var analyzeFile = new AnalyzeFile(
            multiAnalysisId,
            processedFile.Metadata.Name,
            processedFile.Metadata.ContentType,
            processedFile.Metadata.Size,
            processedFile.HashValues.Sha256,
            filePassword,
            isPrivateFile,
            processedFile.StorageFileName);

        ISendEndpoint sendEndpoint = await _sendEndpointProvider.GetSendEndpoint(_endpointUriProvider.AnalyzeFileUri);
        await sendEndpoint.Send(analyzeFile, cancellationToken);
    }
}
using MassTransit;

using Openlysis.Application.Common.Models;
using Openlysis.Application.Files.Contracts.Abstractions;
using Openlysis.Domain.Common.ValueObjects;
using Openlysis.Infrastructure.Shared.Communication.Abstractions;
using Openlysis.Infrastructure.Shared.Communication.Contracts;

namespace Openlysis.Infrastructure.Services.Files;

/// <summary>
/// Provides functionality to queue files for multi-analysis processing.
/// </summary>
internal class FileMultiAnalysisQueue : IFileMultiAnalysisQueue
{
    private readonly IEndpointUriProvider _endpointUriProvider;
    private readonly ISendEndpointProvider _sendEndpointProvider;

    /// <summary>
    /// Initializes a new instance of the <see cref="FileMultiAnalysisQueue"/> class.
    /// </summary>
    /// <param name="endpointUriProvider">The provider for endpoint URIs.</param>
    /// <param name="sendEndpointProvider">The endpoint to send messages to.</param>
    public FileMultiAnalysisQueue(
        IEndpointUriProvider endpointUriProvider,
        ISendEndpointProvider sendEndpointProvider)
    {
        _endpointUriProvider = endpointUriProvider;
        _sendEndpointProvider = sendEndpointProvider;
    }

    /// <inheritdoc/>
    public async Task QueueAsync(
        GlobalId multiAnalysisId,
        ProcessedFile processedFile,
        string filePassword,
        bool isPrivateFile,
        GlobalId? correlationId,
        CancellationToken cancellationToken)
    {
        var analyzeFile = new AnalyzeFileMessage(
            multiAnalysisId,
            processedFile.Metadata.Name,
            processedFile.Metadata.ContentType,
            processedFile.Metadata.Size,
            processedFile.HashValues.Sha256,
            filePassword,
            isPrivateFile,
            processedFile.StorageFileName,
            correlationId);

        ISendEndpoint sendEndpoint = await _sendEndpointProvider.GetSendEndpoint(_endpointUriProvider.AnalyzeFileUri);
        await sendEndpoint.Send(analyzeFile, cancellationToken);
    }
}
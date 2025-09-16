using MessagePack;

using Openlysis.API.Endpoints.Files.Common.Responses;
using Openlysis.API.Endpoints.Messages.Common.Responses;
using Openlysis.API.Endpoints.URLs.Common;

namespace Openlysis.API.Notifications;

/// <summary>
/// Defines methods for receiving analysis update notifications for files, URLs, and messages for the SignalR clients.
/// </summary>
public interface IAnalysisUpdatesClient
{
    /// <summary>
    /// Receives an update for a file multi-analysis operation.
    /// </summary>
    /// <param name="dto">The file multi-analysis data transfer object.</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    public Task ReceiveFileMultiAnalysisUpdate(FileMultiAnalysisDto dto);

    /// <summary>
    /// Receives an update for a URL multi-analysis operation.
    /// </summary>
    /// <param name="dto">The URL multi-analysis data transfer object.</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    public Task ReceiveUrlMultiAnalysisUpdate(UrlMultiAnalysisDto dto);

    /// <summary>
    /// Receives an update for an email analysis operation.
    /// </summary>
    /// <param name="dto">The email analysis data transfer object.</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    public Task ReceiveEmailAnalysisUpdate(MessageAnalysisDto dto);

    /// <summary>
    /// Receives an update for an SMS analysis operation.
    /// </summary>
    /// <param name="dto">The SMS analysis data transfer object.</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    public Task ReceiveSmsAnalysisUpdate(MessageAnalysisDto dto);
}
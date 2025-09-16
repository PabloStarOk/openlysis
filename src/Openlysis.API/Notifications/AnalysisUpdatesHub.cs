using Microsoft.AspNetCore.SignalR;

namespace Openlysis.API.Notifications;

/// <summary>
/// SignalR hub for sending analysis update notifications to connected clients.
/// </summary>
internal sealed class AnalysisUpdatesHub : Hub<IAnalysisUpdatesClient>
{
    /// <summary>
    /// The name of the SignalR hub.
    /// </summary>
    public const string Name = "analysis-updates";
}
namespace Openlysis.API.Notifications;

/// <summary>
/// Contains constants used for notification hubs and related queries.
/// </summary>
internal static class NotificationConstants
{
    /// <summary>
    /// The root path for all notification hubs.
    /// </summary>
    public const string HubsRootPath = "hubs";

    /// <summary>
    /// The query parameter name for access tokens.
    /// </summary>
    public const string AccessTokenQuery = "access_token";

    /// <summary>
    /// The path for the AnalysisUpdatesHub.
    /// </summary>
    public const string AnalysisUpdatesHubPath = $"{HubsRootPath}/{AnalysisUpdatesHub.Name}";
}
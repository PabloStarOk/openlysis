namespace Openlysis.API.Notifications;

/// <summary>
/// Provides extension methods for configuring application-level notifications.
/// </summary>
internal static class AppConfiguration
{
    /// <summary>
    /// Configures the application to use push notifications by mapping the AnalysisUpdatesHub
    /// to the specified path and requiring authorization.
    /// </summary>
    /// <param name="app">The <see cref="WebApplication"/> instance to configure.</param>
    public static void UsePushNotifications(this WebApplication app)
    {
        app.MapHub<AnalysisUpdatesHub>(NotificationConstants.AnalysisUpdatesHubPath).RequireAuthorization();
    }
}
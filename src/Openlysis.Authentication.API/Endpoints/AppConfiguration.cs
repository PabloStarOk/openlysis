using FastEndpoints;

namespace Openlysis.Authentication.API.Endpoints;

/// <summary>
/// Provides extension methods for configuring the API endpoints.
/// </summary>
internal static class AppConfiguration
{
    /// <summary>
    /// Configures FastEndpoints for the given <see cref="WebApplication"/> instance.
    /// </summary>
    /// <param name="app">The web application to configure.</param>
    public static void ConfigureApi(this WebApplication app)
    {
        app.UseFastEndpoints(o =>
        {
            o.Serializer.Options.TypeInfoResolver = ApiJsonSerializerContext.Default;
        });
    }
}
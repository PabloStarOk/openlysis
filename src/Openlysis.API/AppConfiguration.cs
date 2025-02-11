using FastEndpoints;

namespace Openlysis.API;

/// <summary>
/// Configuration of the API for the web application.
/// </summary>
public static class AppConfiguration
{
    /// <summary>
    /// Configures the API for the web application.
    /// </summary>
    /// <param name="app">The WebApplication instance to configure.</param>
    public static void ConfigureApi(this WebApplication app)
    {
        if (app.Environment.IsDevelopment())
        {
            app.UseOpenApi();
            app.UseSwaggerUi(c => c.DocExpansion = "list");
        }

        app.UseFastEndpoints();
    }
}
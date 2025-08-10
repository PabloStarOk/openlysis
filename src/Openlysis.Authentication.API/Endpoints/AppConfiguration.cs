using FastEndpoints;
using FastEndpoints.Swagger;

using Scalar.AspNetCore;

namespace Openlysis.Authentication.API.Endpoints;

/// <summary>
/// Provides extension methods for configuring the API endpoints.
/// </summary>
internal static class AppConfiguration
{
    private const string DocumentationGenerationPath = "/openapi/{documentName}.json";
    private const string ApiDocumentationPath = "api-docs";
    private const string WebPageTitle = "Openlysis Authentication API";
    private const string EndpointSuffix = "Endpoint";

    /// <summary>
    /// Configures FastEndpoints for the given <see cref="WebApplication"/> instance.
    /// </summary>
    /// <param name="app">The web application to configure.</param>
    public static void ConfigureApi(this WebApplication app)
    {
        app.UseFastEndpoints(o =>
        {
            o.Serializer.Options.TypeInfoResolver =
                ApiJsonSerializerContext.Default;
            o.Endpoints.ShortNames = true;

            o.Endpoints.NameGenerator = context
                => context.EndpointType.Name.TrimEnd(EndpointSuffix.ToCharArray());
        });

        if (app.Environment.IsDevelopment())
        {
            app.UseSwaggerGen(o => o.Path = DocumentationGenerationPath);

            app.MapScalarApiReference(ApiDocumentationPath, o =>
            {
                o.WithTitle(WebPageTitle);
                o.HiddenClients = true;
            });
        }
    }
}
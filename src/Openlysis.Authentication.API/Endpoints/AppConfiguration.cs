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
    private const string EndpointNameSuffix = "Endpoint";
    private const string EndpointPathPrefix = "api";
    private const string VersioningPrefix = "v";
    private const int EndpointDefaultVersion = 1;

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
                => context.EndpointType.Name.TrimEnd(EndpointNameSuffix.ToCharArray());
            o.Endpoints.RoutePrefix = EndpointPathPrefix;

            o.Versioning.DefaultVersion = EndpointDefaultVersion;
            o.Versioning.Prefix = VersioningPrefix;
            o.Versioning.PrependToRoute = true;
        });

        if (app.Environment.IsDevelopment())
        {
            app.UseSwaggerGen(o => o.Path = DocumentationGenerationPath);

            app.MapScalarApiReference(ApiDocumentationPath, o =>
            {
                o.WithTitle(WebPageTitle);
                o.HiddenClients = true;

                o.AddDocument(DependencyInjection.V1DocumentName);
            });
        }
    }
}
using FastEndpoints;
using FastEndpoints.Swagger;

using NSwag;

namespace Openlysis.API;

/// <summary>
/// Dependency injection of the API.
/// </summary>
public static class DependencyInjection
{
    /// <summary>
    /// Adds all services needed for the API.
    /// </summary>
    /// <param name="serviceCollection">Collection of services.</param>
    public static void AddApi(this IServiceCollection serviceCollection)
    {
        serviceCollection.AddProblemDetails(
            opt =>
            {
                opt.CustomizeProblemDetails = context =>
                {
                    context.ProblemDetails.Instance = context.HttpContext.Request.Path + context.HttpContext.Request.QueryString;
                };
            });

        serviceCollection.AddFastEndpoints(
            opt =>
            {
                opt.DisableAutoDiscovery = true;
                opt.SourceGeneratorDiscoveredTypes.AddRange(typeof(Program).Assembly.DefinedTypes);
            });

        serviceCollection.SwaggerDocument(
            opt =>
            {
                opt.ReleaseVersion = 1;
                opt.DocumentSettings = s =>
                {
                    s.DocumentName = "Version 1";
                    s.Title = "Openlysis API";
                    s.Description = "API of openlysis.";
                    s.Version = "v1";

                    s.PostProcess = document =>
                    {
                        document.Info = new OpenApiInfo
                        {
                            License = new OpenApiLicense
                            {
                                Name = "MIT License",
                                Url = "https://license.md/licenses/mit-license/",
                            },
                        };
                    };
                };
                opt.RemoveEmptyRequestSchema = true;
            });
    }
}
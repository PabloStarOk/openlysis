using FastEndpoints;
using FastEndpoints.Swagger;

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
        serviceCollection.AddFastEndpoints(
            opt =>
            {
                opt.DisableAutoDiscovery = true;
                opt.SourceGeneratorDiscoveredTypes.AddRange(typeof(Program).Assembly.DefinedTypes);
            });
        serviceCollection.SwaggerDocument(
            opt =>
            {
                opt.DocumentSettings = s =>
                {
                    s.Title = "Openlysis API";
                    s.Description = "API of openlysis.";
                };
            });
    }
}
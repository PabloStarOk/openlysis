using FastEndpoints;
using FastEndpoints.Swagger;

using Microsoft.AspNetCore.Http.Json;
using Microsoft.Extensions.Options;

using Openlysis.Authentication.API.Endpoints.Configuration;

namespace Openlysis.Authentication.API.Endpoints;

/// <summary>
/// Provides extension methods for registering API dependencies and configuration.
/// </summary>
internal static class DependencyInjection
{
    private const string DocumentsTitle = "Openlysis Authentication API";

    /// <summary>
    /// Adds API-related services and configuration to the service collection.
    /// </summary>
    /// <param name="services">The service collection to add services to.</param>
    /// <param name="configuration">The application configuration.</param>
    public static void AddApi(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        AddPasswordRequirements(services, configuration);

        services.Configure<JsonOptions>(o =>
        {
            o.SerializerOptions.TypeInfoResolver = ApiJsonSerializerContext.Default;
        });

        services.AddFastEndpoints(o =>
            o.SourceGeneratorDiscoveredTypes = DiscoveredTypes.All);

        services.SwaggerDocument(o =>
            {
                o.DocumentSettings = settings =>
                {
                    settings.Title = DocumentsTitle;
                    settings.Description = "Authentication API for Openlysis.";
                };
                o.ShortSchemaNames = true;
                o.EnableJWTBearerAuth = false;
                o.ReleaseVersion = 1;
                o.RemoveEmptyRequestSchema = true;
            });
    }

    private static void AddPasswordRequirements(
        IServiceCollection services,
        IConfiguration configuration)
    {
        var optionsSection = configuration
            .GetRequiredSection(PasswordRequirements.SectionName);

        services.AddSingleton<
            IValidateOptions<PasswordRequirements>,
            PasswordRequirementsValidator>();

        services.AddOptions<PasswordRequirements>()
            .Bind(optionsSection)
            .ValidateOnStart();
    }
}
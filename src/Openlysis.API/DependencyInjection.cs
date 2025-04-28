using System.Text.Json;

using FastEndpoints;
using FastEndpoints.Swagger;

using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Server.Kestrel.Core;

using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;

using NSwag;

using Openlysis.API.Authentication;
using Openlysis.API.Authentication.API;
using Openlysis.API.Configuration.Options;
using Openlysis.API.Middlewares.Exceptions;

namespace Openlysis.API;

/// <summary>
/// Dependency injection of the API.
/// </summary>
public static class DependencyInjection
{
    /// <summary>
    /// Adds all services needed for the API.
    /// </summary>
    /// <param name="services">Collection of services.</param>
    /// <param name="configuration">Configuration settings.</param>
    /// <param name="environment">Hosting environment information.</param>
    public static void AddApi(
        this IServiceCollection services,
        IConfiguration configuration,
        IWebHostEnvironment environment)
    {
        // Get options
        var serverOptions = configuration
            .GetRequiredSection(ServerOptions.SectionName)
            .Get<ServerOptions>();

        var fileUploadOptionsSection = configuration
            .GetRequiredSection(FileUploadOptions.SectionName);
        var fileUploadOptions = fileUploadOptionsSection
            .Get<FileUploadOptions>();

        ArgumentNullException.ThrowIfNull(serverOptions);
        ArgumentNullException.ThrowIfNull(fileUploadOptionsSection);
        ArgumentNullException.ThrowIfNull(fileUploadOptions);

        // Add options
        services.Configure<FileUploadOptions>(fileUploadOptionsSection);

        // Server options
        services.Configure<KestrelServerOptions>(
            options =>
            {
                options.Limits.MaxRequestBodySize = serverOptions.MaxRequestBodySize;
            });

        // Request options
        services.Configure<FormOptions>(
            options =>
            {
                options.MultipartBodyLengthLimit = fileUploadOptions.MaxFileSize;
                options.MemoryBufferThreshold = fileUploadOptions.MemoryBufferThreshold;
            });

        // Add authentication and authorization
        services.AddApiAuthentication(configuration, environment);

        services.AddProblemDetails(
            opt =>
            {
                opt.CustomizeProblemDetails = context =>
                {
                    context.ProblemDetails.Instance = context.HttpContext.Request.Path + context.HttpContext.Request.QueryString;
                };
            });

        services.AddFastEndpoints(
            opt =>
            {
                opt.DisableAutoDiscovery = true;
                opt.SourceGeneratorDiscoveredTypes.AddRange(typeof(Program).Assembly.DefinedTypes);
                opt.MapAuthenticationEndpoints();
            });

        services.SwaggerDocument(
            opt =>
            {
                opt.ReleaseVersion = 1;
                opt.EnableJWTBearerAuth = false;
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

                    s.AddAuth("API Key", new OpenApiSecurityScheme
                        {
                            Name = "X-Api-Key",
                            In = OpenApiSecurityApiKeyLocation.Header,
                            Type = OpenApiSecuritySchemeType.ApiKey,
                            Description = "API Key authentication.",
                        });
                };

                opt.SerializerSettings = s =>
                {
                    s.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
                };

                opt.NewtonsoftSettings = s =>
                {
                    s.Converters.Add(new StringEnumConverter(new CamelCaseNamingStrategy(), false));
                };

                opt.ShortSchemaNames = true;
                opt.RemoveEmptyRequestSchema = true;
            });

        services.AddExceptionHandler<GlobalExceptionHandler>();
    }
}
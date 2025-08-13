using System.Text.Json;

using FastEndpoints;
using FastEndpoints.Swagger;

using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Server.Kestrel.Core;

using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;

using NSwag;

using Openlysis.API.Configuration.Options;
using Openlysis.API.Middlewares.Exceptions;

namespace Openlysis.API;

/// <summary>
/// Dependency injection of the API.
/// </summary>
public static class DependencyInjection
{
    /// <summary>
    /// The name of the Swagger document for API version 1.
    /// </summary>
    public const string V1DocumentName = "Version 1";

    private const string DocumentsTitle = "Openlysis Authentication API";
    private const string ApiDescription = "Authentication API for Openlysis.";
    private const string V1DocumentVersion = "v1";

    /// <summary>
    /// Adds all services needed for the API.
    /// </summary>
    /// <param name="services">Collection of services.</param>
    /// <param name="configuration">Configuration settings.</param>
    /// <param name="environment">Hosting environment information.</param>
    public static void AddApi(
        this IServiceCollection services,
        IConfiguration configuration,
        IHostEnvironment environment)
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

        AddJwtAuthentication(services, configuration, environment);
        services.AddAuthorization();

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
            });

        services.SwaggerDocument(
            opt =>
            {
                opt.ReleaseVersion = 1;
                opt.DocumentSettings = s =>
                {
                    s.DocumentName = V1DocumentName;
                    s.Title = DocumentsTitle;
                    s.Description = ApiDescription;
                    s.Version = V1DocumentVersion;
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

        services.AddExceptionHandlers();
    }

    private static void AddJwtAuthentication(
        IServiceCollection services,
        IConfiguration configuration,
        IHostEnvironment environment)
    {
        var jwtBearerOptions = configuration
            .GetRequiredSection(nameof(JwtBearerOptions))
            .Get<JwtBearerOptions>();
        ArgumentNullException.ThrowIfNull(jwtBearerOptions);

        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
                {
                    options.RequireHttpsMetadata = environment.IsProduction() || jwtBearerOptions.RequireHttpsMetadata;

                    options.Authority = jwtBearerOptions.Authority;
                    options.MetadataAddress = jwtBearerOptions.MetadataAddress;
                    options.TokenValidationParameters = jwtBearerOptions.TokenValidationParameters;
                });
    }
}
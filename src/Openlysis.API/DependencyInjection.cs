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
using Openlysis.API.Documentation;
using Openlysis.API.Endpoints.Files.Analyze;
using Openlysis.API.Endpoints.Messages.Analyze;
using Openlysis.API.Middlewares.Exceptions;
using Openlysis.API.Middlewares.Files;
using Openlysis.API.Services.Implementations;

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

    private const string DocumentsTitle = "Openlysis Analysis API";
    private const string ApiDescription = "Analysis API of Openlysis.";
    private const string V1DocumentVersion = "v1";

    /// <summary>
    /// Adds all services needed for the API.
    /// </summary>
    /// <param name="services">Collection of services.</param>
    /// <param name="configuration">Configuration settings.</param>
    /// <param name="environment">Hosting environment information.</param>
    internal static void AddApi(
        this IServiceCollection services,
        IConfiguration configuration,
        IHostEnvironment environment)
    {
        // Get options
        var serverOptions = configuration
            .GetRequiredSection(ServerOptions.SectionName)
            .Get<ServerOptions>();

        var messageAnalysisOptionsSection = configuration
            .GetRequiredSection(MessageAnalysisOptions.SectionName);
        var fileUploadOptions = messageAnalysisOptionsSection
            .Get<MessageAnalysisOptions>();

        ArgumentNullException.ThrowIfNull(serverOptions);
        ArgumentNullException.ThrowIfNull(messageAnalysisOptionsSection);
        ArgumentNullException.ThrowIfNull(fileUploadOptions);

        // Add options
        services.Configure<MessageAnalysisOptions>(messageAnalysisOptionsSection);

        // Server options
        services.Configure<KestrelServerOptions>(
            options =>
            {
                options.Limits.MaxRequestBodySize = serverOptions.MaxRequestBodySize;
            });

        AddMultipartRequestBinders(services, configuration);

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
                    s.SchemaSettings.SchemaNameGenerator = new SchemaNameGenerator();
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
        services.AddScoped<FileStorageCleanupMiddleware>();
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

    private static void AddMultipartRequestBinders(
        IServiceCollection services,
        IConfiguration configuration)
    {
        var formOptionsSection = configuration.GetRequiredSection(nameof(FormOptions));

        services.AddOptions<FormOptions>()
            .Bind(formOptionsSection)
            .ValidateOnStart();

        services.AddSingleton<IRequestBinder<AnalyzeFileRequest>, AnalyzeFileMultipartRequestBinder>();
        services.AddSingleton<IRequestBinder<AnalyzeMessageRequest>, AnalyzeMessageMultipartRequestBinder>();
    }
}
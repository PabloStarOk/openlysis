using FastEndpoints;
using FastEndpoints.Swagger;

using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Server.Kestrel.Core;

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
    /// Adds all services needed for the API.
    /// </summary>
    /// <param name="serviceCollection">Collection of services.</param>
    /// <param name="configuration">Configuration settings.</param>
    public static void AddApi(
        this IServiceCollection serviceCollection,
        IConfiguration configuration)
    {
        var fileUploadOptions = configuration
            .GetRequiredSection("FileUploadOptions")
            .Get<FileUploadOptions>();
        ArgumentNullException.ThrowIfNull(fileUploadOptions);

        serviceCollection.Configure<KestrelServerOptions>(
            options =>
            {
                options.Limits.MaxRequestBodySize = fileUploadOptions.MaxRequestBodySize;
            });

        serviceCollection.Configure<FormOptions>(
            options =>
            {
                options.MemoryBufferThreshold = fileUploadOptions.MemoryBufferThreshold;
            });

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

        serviceCollection.AddExceptionHandler<GlobalExceptionHandler>();
    }
}
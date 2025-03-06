using FastEndpoints;
using FastEndpoints.Swagger;

using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Server.Kestrel.Core;

using NSwag;

using Openlysis.API.Configuration.Options;
using Openlysis.API.Middlewares.Exceptions;
using Openlysis.Infrastructure.Persistence;

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
        var fileUploadOptions = configuration
            .GetRequiredSection("FileUploadOptions")
            .Get<FileUploadOptions>();
        ArgumentNullException.ThrowIfNull(fileUploadOptions);

        // Server options
        services.Configure<KestrelServerOptions>(
            options =>
            {
                options.Limits.MaxRequestBodySize = fileUploadOptions.MaxRequestBodySize;
            });

        // Auth options
        services.AddAuthorization();
        services.AddIdentityApiEndpoints<IdentityUser>(
                opt =>
                {
                    opt.User.RequireUniqueEmail = true;

                    if (environment.IsDevelopment())
                    {
                        return;
                    }

                    opt.SignIn.RequireConfirmedEmail = true;
                    opt.SignIn.RequireConfirmedAccount = true;
                    opt.Password.RequiredLength = 8;
                })
            .AddEntityFrameworkStores<AuthenticationDbContext>();

        // Request options
        services.Configure<FormOptions>(
            options =>
            {
                options.MemoryBufferThreshold = fileUploadOptions.MemoryBufferThreshold;
            });

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

        services.AddExceptionHandler<GlobalExceptionHandler>();
    }
}
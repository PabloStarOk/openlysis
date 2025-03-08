using FastEndpoints;

using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

using Openlysis.API.Authentication.API.Configuration.Options.Authentication;
using Openlysis.API.Authentication.Application.Services.Interfaces;
using Openlysis.API.Authentication.Infrastructure.Persistence;
using Openlysis.API.Authentication.Infrastructure.Persistence.Models;
using Openlysis.API.Authentication.Infrastructure.Services;

namespace Openlysis.API.Authentication.API;

/// <summary>
/// Provides methods to add API services and configurations to the IServiceCollection.
/// </summary>
public static class DependencyInjection
{
    /// <summary>
    /// Adds API services and configurations to the specified IServiceCollection.
    /// </summary>
    /// <param name="services">The IServiceCollection to add services to.</param>
    /// <param name="environment">The IWebHostEnvironment providing information about the hosting environment.</param>
    public static void AddApi(
        this IServiceCollection services,
        IWebHostEnvironment environment)
    {
        // Add services
        services.AddTransient<IApiKeyHasher, ApiKeyHasher>();
        services.AddTransient<IApiKeyProvider, ApiKeyProvider>();

        // Add authentication middleware.
        services.AddAuthentication()
            .AddScheme<ApiKeySchemeOptions, ApiKeySchemeHandler>(
                ApiKeySchemeOptions.Scheme,
                options =>
                {
                    options.HeaderName = "X-Api-Key";
                });

        // Add authorization middleware.
        services.AddAuthorizationBuilder()
            .AddPolicy("AuthScheme", configure =>
            {
                configure.RequireAuthenticatedUser();
                configure.AddAuthenticationSchemes(ApiKeySchemeOptions.Scheme);
            });

        services.AddIdentityCore<User>(
                options =>
                {
                    options.User.RequireUniqueEmail = true;

                    if (environment.IsDevelopment())
                    {
                        return;
                    }

                    options.SignIn.RequireConfirmedEmail = true;
                    options.SignIn.RequireConfirmedAccount = true;
                    options.Password.RequiredLength = 8;
                })
            .AddSignInManager<SignInManager<User>>()
            .AddEntityFrameworkStores<AuthenticationDbContext>();
    }

    /// <summary>
    /// Maps FastEndpoints discovered by the source generator to the EndpointDiscoveryOptions.
    /// </summary>
    /// <param name="options">The EndpointDiscoveryOptions to add discovered endpoints to.</param>
    public static void MapAuthenticationEndpoints(this EndpointDiscoveryOptions options)
    {
        options.SourceGeneratorDiscoveredTypes.AddRange(typeof(Openlysis.API.Authentication.DependencyInjection).Assembly.DefinedTypes);
    }
}
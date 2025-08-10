using Microsoft.Extensions.Options;

using Openlysis.Authentication.API.Application.Common.Configuration;
using Openlysis.Authentication.API.Application.Common.Services;
using Openlysis.Authentication.API.Application.Refresh;
using Openlysis.Authentication.API.Application.SignIn;
using Openlysis.Authentication.API.Application.SignUp;

namespace Openlysis.Authentication.API.Application;

/// <summary>
/// Provides extension methods for registering application-layer services.
/// </summary>
internal static class DependencyInjection
{
    /// <summary>
    /// Registers application services with the dependency injection container.
    /// </summary>
    /// <param name="services">The service collection to add services to.</param>
    /// <param name="configuration">The application configuration instance.</param>
    public static void AddApplication(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddTransient<RefreshTokenStore>();
        services.AddTransient<ISignUpService, SignUpService>();
        services.AddTransient<ITokenSignInService, TokenSignInService>();
        AddSignInRefreshmentService(services, configuration);
    }

    private static void AddSignInRefreshmentService(
        IServiceCollection services,
        IConfiguration configuration)
    {
        var optionsSection = configuration
            .GetRequiredSection(RefreshTokenOptions.SectionName);

        services.AddSingleton<
            IValidateOptions<RefreshTokenOptions>,
            RefreshTokenOptionsValidator>();

        services.AddOptions<RefreshTokenOptions>()
            .Bind(optionsSection)
            .ValidateOnStart();

        services.AddTransient<ISignInTokenRefreshService, SignInTokenRefreshService>();
    }
}
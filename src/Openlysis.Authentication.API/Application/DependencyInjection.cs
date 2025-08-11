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
    public static void AddApplication(
        this IServiceCollection services)
    {
        services.AddTransient<RefreshTokenStore>();
        services.AddTransient<ISignUpService, SignUpService>();
        services.AddTransient<ITokenSignInService, TokenSignInService>();
        services.AddTransient<ISignInTokenRefreshService, SignInTokenRefreshService>();
    }
}
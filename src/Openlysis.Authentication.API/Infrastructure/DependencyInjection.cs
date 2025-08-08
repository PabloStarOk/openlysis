using Microsoft.Extensions.Options;

using Openlysis.Authentication.API.Application.Common.Abstractions.Persistence;
using Openlysis.Authentication.API.Application.Common.Abstractions.Services;
using Openlysis.Authentication.API.Infrastructure.Configuration;
using Openlysis.Authentication.API.Infrastructure.Persistence;
using Openlysis.Authentication.API.Infrastructure.Persistence.Repositories;
using Openlysis.Authentication.API.Infrastructure.Services;

namespace Openlysis.Authentication.API.Infrastructure;

/// <summary>
/// Provides extension methods for registering infrastructure (concrete implementation) services.
/// </summary>
internal static class DependencyInjection
{
    private const string AuthConnectionString = "AuthConnection";

    /// <summary>
    /// Registers infrastructure services into the provided <see cref="IServiceCollection"/>.
    /// </summary>
    /// <param name="services">The service collection to add services to.</param>
    /// <param name="configuration">The application configuration instance.</param>
    public static void AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        AddRepository(services, configuration);
        AddPasswordHasher(services, configuration);
    }

    private static void AddRepository(
        IServiceCollection services,
        IConfiguration configuration)
    {
        string? connectionString = configuration.GetConnectionString(AuthConnectionString);
        ArgumentException.ThrowIfNullOrWhiteSpace(connectionString);
        services.AddSingleton(new AuthDbContext(connectionString));

        services.AddTransient<IUserRepository, UserRepository>();
    }

    private static void AddPasswordHasher(
        IServiceCollection services,
        IConfiguration configuration)
    {
        var optionsSection = configuration
            .GetSection(PasswordHasherOptions.SectionName);

        services.AddSingleton<
            IValidateOptions<PasswordHasherOptions>,
            PasswordHasherOptionsValidator>();

        services.AddOptions<PasswordHasherOptions>()
            .Bind(optionsSection)
            .ValidateOnStart();

        services.AddTransient<IPasswordHasher, PasswordHasher>();
    }
}
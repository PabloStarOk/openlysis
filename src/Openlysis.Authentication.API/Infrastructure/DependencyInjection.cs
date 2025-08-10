using System.Data.Common;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;

using Microsoft.Extensions.Options;

using Npgsql;

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
        AddRepositories(services, configuration);
        AddPasswordHasher(services, configuration);
        AddTokenGenerator(services, configuration);
        AddTokenHasher(services);
    }

    private static void AddRepositories(
        IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddSingleton<DbDataSource>(
            sp =>
            {
                string? connectionString = configuration.GetConnectionString(AuthConnectionString);
                ArgumentException.ThrowIfNullOrWhiteSpace(connectionString);
                var loggerFactory = sp.GetService<ILoggerFactory>();
                return new NpgsqlSlimDataSourceBuilder(connectionString)
                    .UseLoggerFactory(loggerFactory)
                    .UseNodaTime()
                    .Build();
            });
        services.AddSingleton<AuthDbContext>();
        services.AddTransient<IUserRepository, UserRepository>();
        services.AddTransient<IRefreshTokenRepository, RefreshTokenRepository>();
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

    private static void AddTokenGenerator(
        IServiceCollection services,
        IConfiguration configuration)
    {
        var certificateOptions = configuration
            .GetRequiredSection(CertificateOptions.SectionName)
            .Get<CertificateOptions>();
        ArgumentNullException.ThrowIfNull(certificateOptions);

        if (string.IsNullOrWhiteSpace(certificateOptions.FilePath))
        {
            throw new ArgumentException($"{nameof(certificateOptions.FilePath)} is required.");
        }

        if (string.IsNullOrWhiteSpace(certificateOptions.FilePassword))
        {
            throw new ArgumentException($"{nameof(certificateOptions.FilePassword)} is required.");
        }

        var optionsSection = configuration
            .GetRequiredSection(JwtGeneratorOptions.SectionName);

        services.AddSingleton<X509Certificate2>(_ =>
            {
                var certBytes = File.ReadAllBytes(certificateOptions.FilePath);
                return new X509Certificate2(
                    certBytes,
                    certificateOptions.FilePassword);
            });

        services.AddSingleton<
            IValidateOptions<JwtGeneratorOptions>,
            JwtGeneratorOptionsValidator>();

        services.AddOptions<JwtGeneratorOptions>()
            .Bind(optionsSection)
            .ValidateOnStart();

        services.AddTransient<ITokenGenerator, JwtGenerator>();
    }

    private static void AddTokenHasher(IServiceCollection services)
    {
        services.AddSingleton<HashAlgorithm>(_ => SHA256.Create());
        services.AddTransient<ITokenHasher, TokenHasher>();
    }
}
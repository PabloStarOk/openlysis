using System.Data.Common;
using System.Security.Cryptography;

using Doppler.NET;

using JWT.Algorithms;

using Microsoft.Extensions.Options;

using NodaTime;

using Npgsql;

using Openlysis.Authentication.API.Application.Common.Abstractions.Persistence;
using Openlysis.Authentication.API.Application.Common.Abstractions.Services;
using Openlysis.Authentication.API.Application.Common.Services;
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
        AddCertificateProvider(services, configuration);
        AddSigningCertificateManager(services);
        AddTokenGenerator(services, configuration);
        AddTokenHasher(services);
        AddSigningKeyProvider(services);
    }

    /// <summary>
    /// Registers health checks for infrastructure dependencies.
    /// </summary>
    /// <param name="builder">The health checks builder to add checks to.</param>
    /// <param name="configuration">The application configuration instance.</param>
    public static void AddInfrastructureHealthChecks(
        this IHealthChecksBuilder builder,
        IConfiguration configuration)
    {
        string? connectionString = configuration.GetConnectionString(AuthConnectionString);
        ArgumentException.ThrowIfNullOrWhiteSpace(connectionString);
        builder.AddNpgSql(connectionString);
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

    private static void AddCertificateProvider(
        IServiceCollection services,
        IConfiguration configuration)
    {
        var certificateOptionsSection = configuration
            .GetRequiredSection(DopplerCertificateOptions.SectionName);

        services.AddOptions<DopplerCertificateOptions>()
            .Bind(certificateOptionsSection)
            .ValidateOnStart();

        services.AddSingleton<
            IValidateOptions<DopplerCertificateOptions>,
            DopplerCertificateOptionsValidator>();

        using (ServiceProvider sp = services.BuildServiceProvider())
        {
            var certOptions = sp.GetRequiredService<IOptions<DopplerCertificateOptions>>();
            string? serviceToken = Environment.GetEnvironmentVariable(certOptions.Value.ServiceTokenEnvVariable);
            ArgumentException.ThrowIfNullOrWhiteSpace(serviceToken);

            services.AddDopplerClient(serviceToken, options =>
            {
                options.ProjectName = certOptions.Value.ProjectName;
                options.ConfigName = certOptions.Value.ConfigName;
            });
        }

        services.AddSingleton<DopplerCertificateProvider>();
        services.AddHostedService(sp => sp.GetRequiredService<DopplerCertificateProvider>());
        services.AddSingleton<ICertificateProvider>(sp => sp.GetRequiredService<DopplerCertificateProvider>());
    }

    private static void AddSigningCertificateManager(IServiceCollection services)
    {
        services.AddSingleton<IJwkGenerator, EcdsaJwkGenerator>();
        services.AddSingleton<SigningCertificateManager>();
    }

    private static void AddTokenGenerator(
        IServiceCollection services,
        IConfiguration configuration)
    {
        var jwtOptionsSection = configuration
            .GetRequiredSection(JwtGeneratorOptions.SectionName);

        var refreshTokenOptionsSection = configuration
            .GetRequiredSection(RefreshTokenOptions.SectionName);

        services.AddSingleton<
            IValidateOptions<JwtGeneratorOptions>,
            JwtGeneratorOptionsValidator>();

        services.AddOptions<JwtGeneratorOptions>()
            .Bind(jwtOptionsSection)
            .ValidateOnStart();

        services.AddSingleton<
            IValidateOptions<RefreshTokenOptions>,
            RefreshTokenOptionsValidator>();

        services.AddOptions<RefreshTokenOptions>()
            .Bind(refreshTokenOptionsSection)
            .ValidateOnStart();

        services.AddSingleton<IClock>(SystemClock.Instance);
        services.AddSingleton<IAlgorithmFactory>(sp =>
            sp.GetRequiredService<SigningCertificateManager>());
        services.AddTransient<ITokenGenerator, JwtGenerator>();
    }

    private static void AddTokenHasher(IServiceCollection services)
    {
        services.AddSingleton<HashAlgorithm>(_ => SHA256.Create());
        services.AddTransient<ITokenHasher, TokenHasher>();
    }

    private static void AddSigningKeyProvider(IServiceCollection services)
    {
        services.AddSingleton<IJwkProvider>(sp =>
            sp.GetRequiredService<SigningCertificateManager>());
    }
}
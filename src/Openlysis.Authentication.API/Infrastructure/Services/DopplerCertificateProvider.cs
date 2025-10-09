using System.Security.Cryptography.X509Certificates;

using Doppler.NET.Abstractions;
using Doppler.NET.Models;

using Microsoft.Extensions.Options;

using Openlysis.Authentication.API.Infrastructure.Configuration;

namespace Openlysis.Authentication.API.Infrastructure.Services;

/// <summary>
/// Provides an implementation of <see cref="ICertificateProvider"/> that retrieves X509 certificates from Doppler secrets manager.
/// Also implements <see cref="IHostedService"/> for certificate fetching on startup and <see cref="IDisposable"/> for certificate disposal.
/// </summary>
internal sealed class DopplerCertificateProvider : ICertificateProvider, IHostedService, IDisposable
{
    private readonly ILogger<DopplerCertificateProvider> _logger;
    private readonly IOptions<DopplerCertificateOptions> _certificateOptions;
    private readonly IDopplerClient _dopplerClient;
    private X509Certificate2? _certificate;

    /// <inheritdoc/>
    public X509Certificate2 Certificate
    {
        get
        {
            if (_certificate is null)
            {
                throw new InvalidOperationException("Certificate has not been set.");
            }

            return _certificate;
        }
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="DopplerCertificateProvider"/> class.
    /// </summary>
    /// <param name="logger">The logger instance for logging operations.</param>
    /// <param name="certificateOptions">The options containing Doppler certificate configuration.</param>
    /// <param name="dopplerClient">The Doppler client used to fetch secrets.</param>
    public DopplerCertificateProvider(
        ILogger<DopplerCertificateProvider> logger,
        IOptions<DopplerCertificateOptions> certificateOptions,
        IDopplerClient dopplerClient)
    {
        _logger = logger;
        _certificateOptions = certificateOptions;
        _dopplerClient = dopplerClient;
    }

    /// <inheritdoc/>
    public async Task StartAsync(CancellationToken cancellationToken)
    {
        var certificateSecretFetchTask = _dopplerClient.GetSecretAsync(
            _certificateOptions.Value.CertificateSecretName, cancellationToken);

        var passwdSecretFetchTask = _dopplerClient.GetSecretAsync(
            _certificateOptions.Value.PasswordSecretName, cancellationToken);

        DopplerSecret? certSecret = await certificateSecretFetchTask;
        DopplerSecret? passwdSecret = await passwdSecretFetchTask;

        if (certSecret is null)
        {
            throw new InvalidOperationException("Certificate secret not found in Doppler.");
        }

        if (passwdSecret is null)
        {
            throw new InvalidOperationException("Password secret not found in Doppler.");
        }

        _certificate = CreateCertificate(certSecret, passwdSecret);
        _logger.LogInformation("X509 certificate from Doppler has been loaded successfully.");
    }

    /// <inheritdoc/>
    public Task StopAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }

    /// <inheritdoc/>
    public void Dispose()
    {
        _certificate?.Dispose();
    }

    private static X509Certificate2 CreateCertificate(
        DopplerSecret cert,
        DopplerSecret passwd)
    {
        byte[] certBytes = Convert.FromBase64String(cert.Value.Raw);
        return new X509Certificate2(
            certBytes,
            passwd.Value.Raw);
    }
}
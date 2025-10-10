using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;

namespace Openlysis.Authentication.API.Infrastructure.Services;

/// <summary>
/// Provides a X509 certificate for authentication purposes in development environments.
/// </summary>
internal sealed class DevelopmentCertificateProvider : ICertificateProvider, IHostedService, IDisposable
{
    private const string EcCurveFriendlyName = "nistP256";
    private const string X500DistinguishedName = "CN=OpenlysisAuth";
    private const int CertificateYearsValidity = 1;
    private const string CertFileName = "dev_x509_certificate.pem";
    private const string PrivateKeyFileName = "dev_ecdsa_key.pem";

    /// <inheritdoc/>
    public X509Certificate2 Certificate
    {
        get
        {
            return _selfSignedCertificate ?? throw new InvalidOperationException("Certificate has not been set.");
        }
    }

    private readonly ILogger<DevelopmentCertificateProvider> _logger;
    private X509Certificate2? _selfSignedCertificate;

    /// <summary>
    /// Initializes a new instance of the <see cref="DevelopmentCertificateProvider"/> class.
    /// </summary>
    /// <param name="logger">The logger instance for logging certificate provider events.</param>
    public DevelopmentCertificateProvider(ILogger<DevelopmentCertificateProvider> logger)
    {
        _logger = logger;
    }

    /// <inheritdoc/>
    public void Dispose()
    {
        _selfSignedCertificate?.Dispose();
    }

    /// <inheritdoc/>
    public async Task StartAsync(CancellationToken cancellationToken)
    {
        string certFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, CertFileName);
        string privateKeyFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, PrivateKeyFileName);

        if (File.Exists(certFilePath) && File.Exists(privateKeyFilePath))
        {
            _selfSignedCertificate = X509Certificate2.CreateFromPemFile(certFilePath, privateKeyFilePath);
            if (_selfSignedCertificate.NotAfter.ToUniversalTime() > DateTime.UtcNow)
            {
                _logger.LogInformation("X509 certificate for development has been loaded successfully from existing certificate.");
                return;
            }
        }

        using var ecDsaKeys = ECDsa.Create(ECCurve.CreateFromFriendlyName(EcCurveFriendlyName));
        _selfSignedCertificate = CreateCertificate(ecDsaKeys);

        string certPem = _selfSignedCertificate.ExportCertificatePem();
        string privateKeyPem = ecDsaKeys.ExportECPrivateKeyPem();

        await File.WriteAllTextAsync(certFilePath, certPem, cancellationToken);
        await File.WriteAllTextAsync(privateKeyFilePath, privateKeyPem, cancellationToken);

        _logger.LogInformation("X509 certificate for development has been created and loaded successfully.");
    }

    /// <inheritdoc/>
    public Task StopAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }

    private static X509Certificate2 CreateCertificate(ECDsa ecDsaKeys)
    {
        var subjectName = new X500DistinguishedName(X500DistinguishedName);
        var request = new CertificateRequest(subjectName, ecDsaKeys, HashAlgorithmName.SHA256);

        request.CertificateExtensions.Add(new X509KeyUsageExtension(X509KeyUsageFlags.DigitalSignature, critical: true));

        DateTimeOffset notBefore = DateTimeOffset.UtcNow;
        DateTimeOffset notAfter = notBefore.AddYears(CertificateYearsValidity);
        return request.CreateSelfSigned(notBefore, notAfter);
    }
}
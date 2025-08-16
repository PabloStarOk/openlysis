namespace Openlysis.Authentication.API.Infrastructure.Configuration;

/// <summary>
/// Represents configuration options for retrieving a certificate from Doppler.
/// </summary>
internal sealed record DopplerCertificateOptions
{
    /// <summary>
    /// The configuration section name for certificate options.
    /// </summary>
    public const string SectionName = "DopplerCertificate";

    /// <summary>
    /// Gets or sets the name of the environment variable containing the Doppler service token.
    /// </summary>
    required public string ServiceTokenEnvVariable { get; set; }

    /// <summary>
    /// Gets or sets the Doppler project name where secrets are stored.
    /// </summary>
    required public string ProjectName { get; set; }

    /// <summary>
    /// Gets or sets the Doppler configuration name to use.
    /// </summary>
    required public string ConfigName { get; set; }

    /// <summary>
    /// Gets or sets the Doppler secret name for the certificate.
    /// </summary>
    required public string CertificateSecretName { get; set; }

    /// <summary>
    /// Gets or sets the Doppler secret name for the certificate password.
    /// </summary>
    required public string PasswordSecretName { get; set; }
}

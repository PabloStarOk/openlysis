namespace Openlysis.Authentication.API.Infrastructure.Configuration;

/// <summary>
/// Options for configuring the certificate used by the authentication API.
/// </summary>
internal sealed record CertificateOptions
{
    /// <summary>
    /// The configuration section name for certificate options.
    /// </summary>
    public const string SectionName = "Certificate";

    /// <summary>
    /// Gets or sets the file path to the certificate.
    /// </summary>
    required public string FilePath { get; set; }

    /// <summary>
    /// Gets or sets the password for the certificate file.
    /// </summary>
    required public string FilePassword { get; set; }
}
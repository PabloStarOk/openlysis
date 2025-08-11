namespace Openlysis.Authentication.API.Infrastructure.Configuration;

/// <summary>
/// Options for configuring the certificate used by the authentication API.
/// </summary>
/// <param name="FilePath">The file path to the certificate.</param>
/// <param name="FilePassword">The password for the certificate file.</param>
internal sealed record CertificateOptions(
    string FilePath,
    string FilePassword)
{
    /// <summary>
    /// The configuration section name for certificate options.
    /// </summary>
    public const string SectionName = "Certificate";
}

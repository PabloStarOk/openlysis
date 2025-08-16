using System.Security.Cryptography.X509Certificates;

namespace Openlysis.Authentication.API.Infrastructure.Services;

/// <summary>
/// Provides access to an X509 certificate for signing JWT.
/// </summary>
internal interface ICertificateProvider
{
    /// <summary>
    /// Gets the X509 certificate used for signing JWTs.
    /// </summary>
    public X509Certificate2 Certificate { get; }
}
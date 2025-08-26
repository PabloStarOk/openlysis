using Google.Apis.Auth.OAuth2;

namespace Openlysis.Infrastructure.Shared.Communication.Services.Files;

/// <summary>
/// Provides Google Cloud credentials for authentication.
/// </summary>
internal interface IGoogleCloudCredentialProvider
{
    /// <summary>
    /// Gets the Google Cloud credential.
    /// </summary>
    public ICredential Credential { get; }
}

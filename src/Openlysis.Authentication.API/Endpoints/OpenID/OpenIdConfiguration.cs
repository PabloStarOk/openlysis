namespace Openlysis.Authentication.API.Endpoints.OpenID;

/// <summary>
/// A basic configuration based on OpenID Connect for the authentication API.
/// </summary>
internal sealed class OpenIdConfiguration
{
    /// <summary>
    /// The configuration section name for OpenID Connect settings.
    /// </summary>
    public const string SectionName = "OpenIdConfiguration";

    /// <summary>
    /// Gets or sets the issuer identifier for the OpenID provider.
    /// </summary>
    required public string Issuer { get; set; }

    /// <summary>
    /// Gets or sets the endpoint URI for token requests.
    /// </summary>
    required public Uri TokenEndpoint { get; set; }

    /// <summary>
    /// Gets or sets the URI for the JSON Web Key Set (JWKS) document.
    /// </summary>
    required public Uri JwksUri { get; set; }

    /// <summary>
    /// Gets or sets the subject types supported by the provider.
    /// </summary>
    required public string[] SubjectTypesSupported { get; set; }

    /// <summary>
    /// Gets or sets the signing algorithms supported for ID tokens.
    /// </summary>
    required public string[] IdTokenSigningAlgValuesSupported { get; set; }
}
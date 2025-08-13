using System.Text.Json.Serialization;

namespace Openlysis.Authentication.API.Endpoints.OpenID;

/// <summary>
/// A basic configuration based on OpenID Connect for the authentication API.
/// </summary>
internal sealed record OpenIdConfiguration
{
    /// <summary>
    /// The configuration section name for OpenID Connect settings.
    /// </summary>
    public const string SectionName = "OpenIdConfiguration";

    /// <summary>
    /// Gets or sets the issuer identifier for the OpenID provider.
    /// </summary>
    [JsonPropertyName("issuer")]
    required public string Issuer { get; set; }

    /// <summary>
    /// Gets or sets the endpoint URI for token requests.
    /// </summary>
    [JsonPropertyName("token_endpoint")]
    required public Uri TokenEndpoint { get; set; }

    /// <summary>
    /// Gets or sets the URI for the JSON Web Key Set (JWKS) document.
    /// </summary>
    [JsonPropertyName("jwks_uri")]
    required public Uri JwksUri { get; set; }

    /// <summary>
    /// Gets or sets the subject types supported by the provider.
    /// </summary>
    [JsonPropertyName("subject_types_supported")]
    required public string[] SubjectTypesSupported { get; set; }

    /// <summary>
    /// Gets or sets the signing algorithms supported for ID tokens.
    /// </summary>
    [JsonPropertyName("id_token_signing_alg_values_supported")]
    required public string[] IdTokenSigningAlgValuesSupported { get; set; }
}
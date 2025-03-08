using Microsoft.AspNetCore.Authentication;
using Microsoft.Net.Http.Headers;

namespace Openlysis.API.Configuration.Options.Authentication;

/// <summary>
/// Options for configuring JWT authentication.
/// </summary>
public class ApiKeySchemeOptions : AuthenticationSchemeOptions
{
    /// <summary>
    /// The authentication scheme name for API key authentication.
    /// </summary>
    public const string Scheme = "ApiKeyScheme";

    /// <summary>
    /// Gets or sets the name of the header where the API key is expected.
    /// </summary>
    public string HeaderName { get; set; } = HeaderNames.Authorization;
}
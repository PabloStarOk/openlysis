using System.Security.Claims;
using System.Text.Encodings.Web;

using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Primitives;

using Openlysis.API.Authentication.API.Configuration.Options.Authentication;
using Openlysis.API.Authentication.Application.Services.Interfaces;
using Openlysis.API.Authentication.Models;

namespace Openlysis.API.Authentication.Infrastructure.Services;

/// <summary>
/// Handles the API key authentication scheme.
/// </summary>
/// <remarks>
/// This class is responsible for validating API keys and creating authentication tickets.
/// </remarks>
public class ApiKeySchemeHandler : AuthenticationHandler<ApiKeySchemeOptions>
{
    private readonly UserManager<User> _userManager;
    private readonly IApiKeyHasher _apiKeyHasher;

    /// <summary>
    /// Initializes a new instance of the <see cref="ApiKeySchemeHandler"/> class.
    /// </summary>
    /// <param name="options">The options monitor for the API key scheme.</param>
    /// <param name="logger">The logger factory.</param>
    /// <param name="encoder">The URL encoder.</param>
    /// <param name="userManager">The user manager.</param>
    /// <param name="apiKeyHasher">The API key hasher.</param>
    public ApiKeySchemeHandler(
        IOptionsMonitor<ApiKeySchemeOptions> options,
        ILoggerFactory logger,
        UrlEncoder encoder,
        UserManager<User> userManager,
        IApiKeyHasher apiKeyHasher)
        : base(options, logger, encoder)
    {
        _userManager = userManager;
        _apiKeyHasher = apiKeyHasher;
    }

    /// <inheritdoc/>
    protected override async Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        if (!Request.Headers.TryGetValue(Options.HeaderName, out StringValues headerValue))
        {
            return AuthenticateResult.Fail($"{Options.HeaderName} header was not found.");
        }

        string? apiKey = headerValue;
        if (string.IsNullOrWhiteSpace(apiKey))
        {
            return AuthenticateResult.Fail("API Key was not provided.");
        }

        string apiKeyHash = await _apiKeyHasher.HashAsync(apiKey);
        User? user = await _userManager.Users.SingleOrDefaultAsync(u => u.ApiKeyHash == apiKeyHash);

        if (user is null)
        {
            return AuthenticateResult.Fail("Unauthorized.");
        }

        if (user.Email is null)
        {
            throw new InvalidOperationException("Email of an user was null.");
        }

        if (user.UserName is null)
        {
            throw new InvalidOperationException("UserName of user was null.");
        }

        Claim[] claims =
        [
            new (ClaimTypes.IsPersistent, "false"),
            new (ClaimTypes.Name, user.UserName),
            new (ClaimTypes.Email, user.Email),
        ];
        var claimsIdentity = new ClaimsIdentity(claims, Scheme.Name);
        var claimsPrincipal = new ClaimsPrincipal(claimsIdentity);
        var ticket = new AuthenticationTicket(claimsPrincipal, Scheme.Name);
        return AuthenticateResult.Success(ticket);
    }
}

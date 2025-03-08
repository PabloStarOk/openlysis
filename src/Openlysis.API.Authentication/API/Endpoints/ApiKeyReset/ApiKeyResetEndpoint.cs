using FastEndpoints;

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;

using Openlysis.API.Authentication.API.Endpoints.SignIn;
using Openlysis.API.Authentication.API.Extensions;
using Openlysis.API.Authentication.Application.Services.Interfaces;
using Openlysis.API.Authentication.Infrastructure.Persistence.Models;

namespace Openlysis.API.Authentication.API.Endpoints.ApiKeyReset;

/// <summary>
/// Endpoint for resetting an API key.
/// </summary>
/// <remarks>
/// This endpoint handles the process of resetting an API key for a user. It verifies the user's credentials,
/// including optional two-factor authentication, and generates a new API key if the authentication is successful.
/// </remarks>
public class ApiKeyResetEndpoint : Endpoint<SignInRequest, ApiKeyResponse>
{
    private readonly ILogger<ApiKeyResetEndpoint> _logger;
    private readonly SignInManager<User> _signInManager;
    private readonly IApiKeyProvider _apiKeyProvider;
    private readonly IApiKeyHasher _apiKeyHasher;

    /// <summary>
    /// Initializes a new instance of the <see cref="ApiKeyResetEndpoint"/> class.
    /// </summary>
    /// <param name="logger">The logger instance for logging information.</param>
    /// <param name="signInManager">The sign-in manager for user authentication.</param>
    /// <param name="apiKeyProvider">The API key provider for generating new API keys.</param>
    /// <param name="apiKeyHasher">The API key hasher for hashing API keys.</param>
    public ApiKeyResetEndpoint(
        ILogger<ApiKeyResetEndpoint> logger,
        SignInManager<User> signInManager,
        IApiKeyProvider apiKeyProvider,
        IApiKeyHasher apiKeyHasher)
    {
        _logger = logger;
        _signInManager = signInManager;
        _apiKeyProvider = apiKeyProvider;
        _apiKeyHasher = apiKeyHasher;
    }

    /// <inheritdoc/>
    public override void Configure()
    {
        Post("api-key-reset");
        Version(1);
        Group<AuthenticationGroup>();
        Description(
            builder =>
            {
                builder.WithName("ApiKeyReset");
                builder.WithDisplayName("ApiKeyReset");
                builder.Accepts<SignInRequest>("application/json");
                builder.Produces<ApiKeyResponse>();
                builder.ProducesValidationProblem();
                builder.Produces(StatusCodes.Status401Unauthorized);
                builder.ProducesProblemDetails(StatusCodes.Status404NotFound);
                builder.ProducesProblemDetails(StatusCodes.Status500InternalServerError);
            },
            clearDefaults: true);
        Summary(
            summary =>
            {
                summary.Summary = "Reset API Key";
                summary.Description = "Revokes current API Key and generates a new one for the user.";
                summary.ExampleRequest = new SignInRequest("ExampleUser", "S4mpleP4sswd$", "123456", "XXXX-XXXX-XXXX");
            });
        DontThrowIfValidationFails();
    }

    /// <inheritdoc/>
    public override async Task HandleAsync(SignInRequest request, CancellationToken ct)
    {
        if (ValidationFailed)
        {
            await SendResultAsync(ValidationFailures.AsValidationProblem());
            return;
        }

        User? user = await _signInManager.UserManager.FindByNameAsync(request.UserName);
        if (user is null)
        {
            await SendUnauthorizedAsync(ct);
            return;
        }

        SignInResult signInResult = await _signInManager.CheckPasswordSignInAsync(user, request.Password, false);

        if (signInResult.RequiresTwoFactor)
        {
            if (request.TwoFactorCode is not null)
            {
                signInResult = await _signInManager.TwoFactorAuthenticatorSignInAsync(request.TwoFactorCode, false, false);
            }
            else if (request.TwoFactorRecoveryCode is not null)
            {
                signInResult = await _signInManager.TwoFactorRecoveryCodeSignInAsync(request.TwoFactorRecoveryCode);
            }
        }

        if (!signInResult.Succeeded)
        {
            IResult result = Results.Problem(
                signInResult.ToString(),
                statusCode: StatusCodes.Status401Unauthorized);
            await SendResultAsync(result);
            return;
        }

        if (user.ApiKeyHash is null)
        {
            IResult result = Results.Problem(
                statusCode: StatusCodes.Status404NotFound,
                title: "API Key Not Found.",
                detail: "Create an API Key first by authenticating.");
            await SendResultAsync(result);
            return;
        }

        string newApiKey = _apiKeyProvider.Create();
        string newApiKeyHash = await _apiKeyHasher.HashAsync(newApiKey, ct);
        user.ApiKeyHash = newApiKeyHash;
        IdentityResult updateResult = await _signInManager.UserManager.UpdateAsync(user);
        if (!updateResult.Succeeded)
        {
            _logger.LogError("An error occurred while updating user's information. {Errors}", updateResult.Errors);
            IResult internalError = Results.Problem(
                statusCode: StatusCodes.Status500InternalServerError,
                detail: "An error occurred while resetting the API Key, try again later.");
            await SendResultAsync(internalError);
            return;
        }

        Response = new ApiKeyResponse(newApiKey);
        await SendOkAsync(Response, ct);
    }
}

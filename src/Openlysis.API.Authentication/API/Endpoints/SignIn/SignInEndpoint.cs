using FastEndpoints;

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;

using Openlysis.API.Authentication.API.Extensions;
using Openlysis.API.Authentication.Application.Services.Interfaces;
using Openlysis.API.Authentication.Models;

namespace Openlysis.API.Authentication.API.Endpoints.SignIn;

/// <summary>
/// Endpoint for handling API key sign-in requests.
/// </summary>
public class SignInEndpoint : Endpoint<SignInRequest, ApiKeyResponse>
{
    private readonly ILogger<SignInEndpoint> _logger;
    private readonly SignInManager<User> _signInManager;
    private readonly IApiKeyProvider _apiKeyProvider;
    private readonly IApiKeyHasher _apiKeyHasher;

    /// <summary>
    /// Initializes a new instance of the <see cref="SignInEndpoint"/> class.
    /// </summary>
    /// <param name="logger">The logger instance for logging information.</param>
    /// <param name="signInManager">The sign-in manager for handling user sign-in operations.</param>
    /// <param name="apiKeyProvider">The provider for generating API keys.</param>
    /// <param name="apiKeyHasher">The hasher for generating API key hashes.</param>
    public SignInEndpoint(
        ILogger<SignInEndpoint> logger,
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
        Post("sign-in");
        Version(1);
        Group<AuthenticationGroup>();
        Description(
            builder =>
            {
                builder.WithName("SignIn");
                builder.WithDisplayName("SignIn");
                builder.Accepts<SignInRequest>("application/json");
                builder.Produces<ApiKeyResponse>();
                builder.ProducesValidationProblem();
                builder.Produces(StatusCodes.Status401Unauthorized);
                builder.ProducesProblem(StatusCodes.Status409Conflict);
                builder.ProducesProblem(StatusCodes.Status500InternalServerError);
            },
            clearDefaults: true);
        Summary(
            s =>
            {
                s.Summary = "Logins with a user account.";
                s.Description = "Gets an API Key by login with a user account.";
                s.ExampleRequest = new SignInRequest("ExampleUser", "S4mpleP4sswd$", "123456", "XXXX-XXXX-XXXX");
            });
        AllowAnonymous();
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

        SignInResult signInResult = await _signInManager.CheckPasswordSignInAsync(user, request.Password, true);

        if (signInResult.RequiresTwoFactor)
        {
            if (!string.IsNullOrWhiteSpace(request.TwoFactorCode))
            {
                signInResult = await _signInManager.TwoFactorAuthenticatorSignInAsync(request.TwoFactorCode, false, false);
            }
            else if (!string.IsNullOrWhiteSpace(request.TwoFactorRecoveryCode))
            {
                signInResult = await _signInManager.TwoFactorRecoveryCodeSignInAsync(request.TwoFactorRecoveryCode);
            }
        }

        if (signInResult.IsNotAllowed)
        {
            IResult notAllowedResult = await GetNotAllowedProblemAsync(
                user);
            await SendResultAsync(notAllowedResult);
            return;
        }

        if (!signInResult.Succeeded)
        {
            await SendUnauthorizedAsync(ct);
            return;
        }

        if (user.ApiKeyHash is not null)
        {
            IResult result = Results.Problem(detail: "An API Key was already generated", statusCode: StatusCodes.Status409Conflict);
            await SendResultAsync(result);
            return;
        }

        string apiKey = _apiKeyProvider.Create();
        user.ApiKeyHash = await _apiKeyHasher.HashAsync(apiKey, ct);
        IdentityResult updateResult = await _signInManager.UserManager.UpdateAsync(user);
        if (!updateResult.Succeeded)
        {
            _logger.LogError("An error occurred while updating user's information. {Errors}", updateResult.Errors);
            IResult internalError = Results.Problem(
                statusCode: StatusCodes.Status500InternalServerError,
                detail: "An error occurred while generating the API Key, try again later.");
            await SendResultAsync(internalError);
            return;
        }

        Response = new ApiKeyResponse(apiKey);
        await SendOkAsync(Response, ct);
    }

    /// <summary>
    /// Generates a problem result indicating that the user is not allowed to sign in.
    /// </summary>
    /// <param name="user">The user attempting to sign in.</param>
    /// <returns>
    /// An <see cref="IResult"/> containing details about the missing requirements
    /// (e.g., unconfirmed email or phone number) preventing the user from signing in.
    /// </returns>
    private async Task<IResult> GetNotAllowedProblemAsync(User user)
    {
        bool mustConfirmEmail = false;
        if (_signInManager.Options.SignIn.RequireConfirmedEmail)
        {
            bool isConfirmed = await _signInManager.UserManager
                .IsEmailConfirmedAsync(user);
            mustConfirmEmail = !isConfirmed;
        }

        bool mustConfirmPhoneNumber = false;
        if (_signInManager.Options.SignIn.RequireConfirmedPhoneNumber)
        {
            bool isConfirmed = await _signInManager.UserManager
                .IsPhoneNumberConfirmedAsync(user);
            mustConfirmPhoneNumber = !isConfirmed;
        }

        List<string> missingRequiredData = new (2);
        if (mustConfirmEmail)
        {
            missingRequiredData.Add("email");
        }

        if (mustConfirmPhoneNumber)
        {
            missingRequiredData.Add("phone number");
        }

        return Results.Problem(
            statusCode: StatusCodes.Status401Unauthorized,
            detail: $"Confirm your {string.Join(" and ", missingRequiredData)} to sign in.");
    }
}

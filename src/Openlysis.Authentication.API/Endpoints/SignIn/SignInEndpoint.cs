using ErrorOr;

using FastEndpoints;

using Openlysis.Authentication.API.Application.SignIn;
using Openlysis.Domain.Users.ValueObjects;

namespace Openlysis.Authentication.API.Endpoints.SignIn;

/// <summary>
/// Endpoint for handling user sign-in requests.
/// </summary>
internal sealed class SignInEndpoint : Endpoint<SignInRequest, SignInResponse>
{
    private readonly ITokenSignInService _tokenSignInService;

    /// <summary>
    /// Initializes a new instance of the <see cref="SignInEndpoint"/> class.
    /// </summary>
    /// <param name="tokenSignInService">Service for handling token-based sign-in operations.</param>
    public SignInEndpoint(ITokenSignInService tokenSignInService)
    {
        _tokenSignInService = tokenSignInService;
    }

    /// <inheritdoc/>
    public override void Configure()
    {
        Post("sign-in");
        AllowAnonymous();
        Description(
            builder =>
            {
                builder.WithName("SignIn");
                builder.WithDisplayName("SignIn");
                builder.Accepts<SignInRequest>("application/json");
                builder.Produces<SignInResponse>(statusCode: 200);
                builder.Produces(statusCode: 401);
                builder.ProducesValidationProblem();
            },
            clearDefaults: true);
        Summary(s =>
        {
            s.Summary = "Authenticates a user and return an access token.";
            s.Description = "Authenticates a user with the provided email and password, returning an access token if successful.";
            s.RequestParam(r => r.Email, "The email address of the user.");
            s.RequestParam(r => r.Password, "The password of the user.");
        });
    }

    /// <inheritdoc/>
    public override async Task HandleAsync(SignInRequest req, CancellationToken ct)
    {
        var email = new EmailAddress(req.Email);
        ErrorOr<string> result =
            await _tokenSignInService.SignInAsync(email, req.Password);

        if (result.IsError)
        {
            await Send.UnauthorizedAsync(CancellationToken.None);
            return;
        }

        var response = new SignInResponse(AccessToken: result.Value);
        await Send.OkAsync(response, CancellationToken.None);
    }
}
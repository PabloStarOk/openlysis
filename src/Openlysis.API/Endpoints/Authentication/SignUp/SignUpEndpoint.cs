using FastEndpoints;

using Microsoft.AspNetCore.Identity;
using Openlysis.API.Endpoints.Authentication.Utilities;

namespace Openlysis.API.Endpoints.Authentication.SignUp;

/// <summary>
/// Endpoint to register a new user.
/// </summary>
public class SignUpEndpoint : Endpoint<SignUpRequest>
{
    private readonly UserManager<IdentityUser> _userManager;

    /// <summary>
    /// Initializes a new instance of the <see cref="SignUpEndpoint"/> class.
    /// </summary>
    /// <param name="userManager">The user manager to handle user operations.</param>
    public SignUpEndpoint(UserManager<IdentityUser> userManager)
    {
        _userManager = userManager;
    }

    /// <inheritdoc />
    public override void Configure()
    {
        Post("sign-up");
        Group<AuthenticationGroup>();
        Description(
            builder =>
            {
                builder.WithName("SignUp");
                builder.WithDisplayName("SignUp");
                builder.Accepts<SignUpRequest>("application/json");
                builder.Produces(StatusCodes.Status200OK);
                builder.ProducesValidationProblem();
            },
            clearDefaults: true);
        Summary(
            s =>
            {
                s.Summary = "Register a new user.";
                s.Description = "Registers a new user.";
                s.ExampleRequest = new SignUpRequest("ExampleUser", "example@example.com", "ExamplePassword1234$&");
            });
        AllowAnonymous();
        DontThrowIfValidationFails();
    }

    /// <inheritdoc />
    public override async Task HandleAsync(SignUpRequest request, CancellationToken ct)
    {
        if (!_userManager.SupportsUserEmail)
        {
            throw new NotSupportedException("A user store with email support is required.");
        }

        if (!_userManager.SupportsUserPassword)
        {
            throw new NotSupportedException("A user store with user password support is required");
        }

        if (ValidationFailed)
        {
            IdentityError[] errors = ValidationFailures.Select(e =>
                new IdentityError
                {
                    Code = e.ErrorCode,
                    Description = e.ErrorMessage,
                }).ToArray();
            var errorResult = IdentityResult.Failed(errors);
            await SendResultAsync(errorResult.AsValidationProblem());
            return;
        }

        var user = new IdentityUser();
        await _userManager.SetUserNameAsync(user, request.UserName);
        await _userManager.SetEmailAsync(user, request.Email);
        IdentityResult result = await _userManager.CreateAsync(user, request.Password);

        if (!result.Succeeded)
        {
            await SendResultAsync(result.AsValidationProblem());
            return;
        }

        // TODO: Add email confirmation.
        await SendOkAsync(CancellationToken.None);
    }
}
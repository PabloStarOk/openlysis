using ErrorOr;

using FastEndpoints;

using Openlysis.Authentication.API.Application.SignUp;
using Openlysis.Domain.Users.ValueObjects;

namespace Openlysis.Authentication.API.Endpoints.SignUp;

/// <summary>
/// Endpoint for handling user sign-up requests.
/// </summary>
internal sealed class SignUpEndpoint : Endpoint<SignUpRequest>
{
    private readonly ISignUpService _signUpService;

    /// <summary>
    /// Initializes a new instance of the <see cref="SignUpEndpoint"/> class.
    /// </summary>
    /// <param name="signUpService">The service responsible for handling user sign-up logic.</param>
    public SignUpEndpoint(ISignUpService signUpService)
    {
        _signUpService = signUpService;
    }

    /// <inheritdoc/>
    public override void Configure()
    {
        Post("/sign-up");
        AllowAnonymous();
        Description(
            builder =>
            {
                builder.WithName("SignUp");
                builder.WithDisplayName("SignUp");
                builder.Accepts<SignUpRequest>("application/json");
                builder.Produces(statusCode: 200);
                builder.Produces(statusCode: 400);
                builder.ProducesValidationProblem();
            },
            clearDefaults: true);
        Summary(s =>
        {
            s.Summary = "Register a new user account.";
            s.Description = "Creates a new user account with the provided email and password.";
            s.RequestParam(r => r.Email, "The email address for the new user.");
            s.RequestParam(r => r.Password, "The password for the new user.");
        });
    }

    /// <inheritdoc/>
    public override async Task HandleAsync(SignUpRequest req, CancellationToken ct)
    {
        EmailAddress emailAddress = new (req.Email);
        ErrorOr<Success> result = await _signUpService.SignUpUserAsync(emailAddress, req.Password);

        if (!result.IsError)
        {
            await Send.OkAsync(CancellationToken.None);
            return;
        }

        Dictionary<string, object?> errors = new ();
        result.Errors.ForEach(e => errors.Add(e.Code, e.Description));
        IResult response = Results.Problem(
            statusCode: StatusCodes.Status400BadRequest,
            detail: "One or more errors occurred during sign up.",
            extensions: new Dictionary<string, object?>
            {
                { "errors", errors },
            });

        await Send.ResultAsync(response);
    }
}
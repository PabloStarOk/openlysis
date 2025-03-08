using FastEndpoints;

using FluentValidation;

namespace Openlysis.API.Authentication.API.Endpoints.SignUp;

/// <summary>
/// Validator for the SignUp request.
/// </summary>
public class SignUpRequestValidator : Validator<SignUpRequest>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="SignUpRequestValidator"/> class.
    /// </summary>
    public SignUpRequestValidator()
    {
        RuleFor(x => x.UserName)
            .Cascade(CascadeMode.Continue)
            .NotEmpty().WithMessage("UserName is required.");

        RuleFor(x => x.Email)
            .Cascade(CascadeMode.Continue)
            .NotEmpty().WithMessage("Email is required.")
            .EmailAddress().WithMessage("Invalid email address.");

        RuleFor(x => x.Password)
            .Cascade(CascadeMode.Continue)
            .NotEmpty().WithMessage("Password is required")
            .MinimumLength(8).WithMessage("Password length must be at least 8 characters.");
    }
}

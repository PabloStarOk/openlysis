using FastEndpoints;

using FluentValidation;

namespace Openlysis.API.Authentication.API.Endpoints.SignIn;

/// <summary>
/// Validator for the <see cref="SignInRequest"/> class.
/// </summary>
public class SignInRequestValidator : Validator<SignInRequest>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="SignInRequestValidator"/> class.
    /// </summary>
    public SignInRequestValidator()
    {
        RuleFor(x => x.UserName)
            .Cascade(CascadeMode.Continue)
            .NotEmpty().WithMessage("UserName is required.");

        RuleFor(x => x.Password)
            .Cascade(CascadeMode.Continue)
            .NotEmpty().WithMessage("Password is required")
            .MinimumLength(8).WithMessage("Password length must be at least 8 characters.");
    }
}
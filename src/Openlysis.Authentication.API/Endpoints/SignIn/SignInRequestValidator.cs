using FastEndpoints;

using FluentValidation;

namespace Openlysis.Authentication.API.Endpoints.SignIn;

/// <summary>
/// Validator for <see cref="SignInRequest"/> using FluentValidation.
/// Ensures that the email and password fields meet required criteria.
/// </summary>
internal sealed class SignInRequestValidator : Validator<SignInRequest>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="SignInRequestValidator"/> class.
    /// Sets up validation rules for sign-in requests.
    /// </summary>
    public SignInRequestValidator()
    {
        RuleFor(x => x.Email)
            .Cascade(CascadeMode.Continue)
            .NotNull().WithMessage("Email address is required.")
            .EmailAddress().WithMessage("Please provide a valid email address.");

        RuleFor(x => x.Password)
            .Cascade(CascadeMode.Continue)
            .NotEmpty().WithMessage("Password is required.");
    }
}
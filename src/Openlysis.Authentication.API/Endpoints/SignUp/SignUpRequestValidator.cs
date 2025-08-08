using FastEndpoints;

using FluentValidation;

using Microsoft.Extensions.Options;

using Openlysis.Authentication.API.Endpoints.Configuration;

namespace Openlysis.Authentication.API.Endpoints.SignUp;

/// <summary>
/// Validator for <see cref="SignUpRequest"/> that enforces email and password requirements.
/// </summary>
internal sealed class SignUpRequestValidator : Validator<SignUpRequest>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="SignUpRequestValidator"/> class,
    /// using the provided password requirements.
    /// </summary>
    /// <param name="passwordRequirements">The password requirements options.</param>
    public SignUpRequestValidator(
        IOptions<PasswordRequirements> passwordRequirements)
    {
        RuleFor(x => x.Email)
            .Cascade(CascadeMode.Continue)
            .NotNull().WithMessage("Email address is required.")
            .EmailAddress().WithMessage("Please provide a valid email address.");

        RuleFor(x => x.Password)
            .Cascade(CascadeMode.Continue)
            .NotEmpty().WithMessage("Password is required.")
            .MinimumLength(passwordRequirements.Value.MinLength)
                .WithMessage($"Password must be at least {passwordRequirements.Value.MinLength} characters long.")
            .Must(HasEnoughLowercase(passwordRequirements.Value.MinLowerChars))
                .WithMessage($"Password must contain at least {passwordRequirements.Value.MinLowerChars} lowercase letter(s).")
            .Must(HasEnoughUppercase(passwordRequirements.Value.MinUpperChars))
                .WithMessage($"Password must contain at least {passwordRequirements.Value.MinUpperChars} uppercase letter(s).")
            .Must(HasEnoughDigits(passwordRequirements.Value.MinDigits))
                .WithMessage($"Password must contain at least {passwordRequirements.Value.MinDigits} digit(s).")
            .Must(HasEnoughSpecial(passwordRequirements.Value.MinSpecialChars))
                .WithMessage($"Password must contain at least {passwordRequirements.Value.MinSpecialChars} special character(s).");
    }

    private static Func<string, bool> HasEnoughLowercase(int min) =>
        password => password.Count(char.IsLower) >= min;

    private static Func<string, bool> HasEnoughUppercase(int min) =>
        password => password.Count(char.IsUpper) >= min;

    private static Func<string, bool> HasEnoughDigits(int min) =>
        password => password.Count(char.IsNumber) >= min;

    private static Func<string, bool> HasEnoughSpecial(int min) =>
        password => password.Count(c => !char.IsLetterOrDigit(c)) >= min;
}
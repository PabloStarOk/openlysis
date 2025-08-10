using FastEndpoints;

using FluentValidation;

namespace Openlysis.Authentication.API.Endpoints.Refresh;

/// <summary>
/// Validator for <see cref="SignInRefreshRequest"/> that ensures the refresh token is provided.
/// </summary>
internal sealed class SignInRefreshRequestValidator : Validator<SignInRefreshRequest>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="SignInRefreshRequestValidator"/> class.
    /// </summary>
    public SignInRefreshRequestValidator()
    {
        RuleFor(x => x.RefreshToken)
            .NotEmpty().WithMessage("Refresh token is required.");
    }
}
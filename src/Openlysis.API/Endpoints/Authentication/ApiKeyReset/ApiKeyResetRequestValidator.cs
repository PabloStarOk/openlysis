using FastEndpoints;

using FluentValidation;

namespace Openlysis.API.Endpoints.Authentication.ApiKeyReset;

/// <summary>
/// Validator for the ApiKeyResetRequest.
/// </summary>
public class ApiKeyResetRequestValidator : Validator<ApiKeyResetRequest>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ApiKeyResetRequestValidator"/> class.
    /// </summary>
    public ApiKeyResetRequestValidator()
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
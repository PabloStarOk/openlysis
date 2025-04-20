using FastEndpoints;

using FluentValidation;

namespace Openlysis.API.Endpoints.Sms.Analyze;

/// <summary>
/// Validator for the <see cref="AnalyzeSmsRequest"/> class.
/// Ensures that the required fields in the request are properly validated.
/// </summary>
public class AnalyzeSmsRequestValidator : Validator<AnalyzeSmsRequest>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="AnalyzeSmsRequestValidator"/> class.
    /// </summary>
    public AnalyzeSmsRequestValidator()
    {
        RuleFor(x => x.Sender)
            .NotEmpty()
            .WithMessage("Sender of the SMS must be provided.");

        RuleFor(x => x.Content)
            .NotEmpty()
            .WithMessage("Content of the SMS must be provided.");

        RuleFor(x => x.CountryCode)
            .Length(2)
            .WithMessage("Country code must be in ISO 1366 alpha-2 format.");
    }
}
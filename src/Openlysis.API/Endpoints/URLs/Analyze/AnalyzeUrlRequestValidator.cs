using FastEndpoints;

using FluentValidation;

namespace Openlysis.API.Endpoints.URLs.Analyze;

/// <summary>
/// Validator for AnalyzeUrlRequest.
/// </summary>
public class AnalyzeUrlRequestValidator : Validator<AnalyzeUrlRequest>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="AnalyzeUrlRequestValidator"/> class.
    /// </summary>
    public AnalyzeUrlRequestValidator()
    {
        RuleFor(x => x.Url)
            .NotEmpty().WithMessage("URL must be provided.");

        RuleFor(x => x.IsPrivate)
            .NotEmpty().WithMessage("Must provide if the analysis is available to other users");
    }
}
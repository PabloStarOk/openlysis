using FastEndpoints;

using FluentValidation;

namespace Openlysis.API.Endpoints.Files.Analyze;

/// <summary>
/// Validator for the <see cref="AnalyzeFileRequest"/> class.
/// Defines validation rules for analyzing file requests.
/// </summary>
public class AnalyzeFileRequestValidator : Validator<AnalyzeFileRequest>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="AnalyzeFileRequestValidator"/> class.
    /// Configures validation rules for the <see cref="AnalyzeFileRequest"/>.
    /// </summary>
    public AnalyzeFileRequestValidator()
    {
        RuleFor(x => x.File)
            .NotNull()
            .WithMessage("Provide a file to be analyzed.")
            .Must(f => f?.Length > 0)
            .WithMessage("Must provide a file to be analyzed with a minimum length of 1 byte.");
    }
}
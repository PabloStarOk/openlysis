using FastEndpoints;

using FluentValidation;

using Microsoft.AspNetCore.Http.Features;
using Microsoft.Extensions.Options;

namespace Openlysis.API.Endpoints.Files.Analyze;

/// <summary>
/// Defines validation rules for analyzing file requests.
/// Validator for the <see cref="AnalyzeFileRequest"/> class.
/// </summary>
public class AnalyzeFileRequestValidator : Validator<AnalyzeFileRequest>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="AnalyzeFileRequestValidator"/> class.
    /// </summary>
    /// <param name="formOptions">Injected form options containing multipart body length limit.</param>
    public AnalyzeFileRequestValidator(IOptions<FormOptions> formOptions)
    {
        RuleFor(x => x.File)
            .Cascade(CascadeMode.Stop)
            .NotNull()
            .WithMessage("Provide a file to be analyzed, ensure the file has a minimum length of 1 byte.")
            .Must(x => x.Metadata.Size <= formOptions.Value.MultipartBodyLengthLimit)
            .WithMessage($"File size must not exceed {formOptions.Value.MultipartBodyLengthLimit} bytes.");
    }
}
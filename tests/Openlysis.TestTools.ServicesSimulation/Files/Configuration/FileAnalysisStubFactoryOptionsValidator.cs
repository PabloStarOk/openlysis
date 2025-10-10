using Microsoft.Extensions.Options;

using Openlysis.TestTools.ServicesSimulation.Common.Configuration;
using Openlysis.TestTools.ServicesSimulation.Common.Extensions;

namespace Openlysis.TestTools.ServicesSimulation.Files.Configuration;

/// <summary>
/// Validates the configuration options for the file analysis stub factory.
/// </summary>
/// <remarks>
/// This validator ensures that the file analysis stub factory options meet all required criteria
/// by validating both the base options and file-specific configuration.
/// </remarks>
internal sealed record FileAnalysisStubFactoryOptionsValidator
    : IValidateOptions<FileAnalysisStubFactoryOptions>
{
    private readonly IValidateOptions<AnalysisStubFactoryOptions> _baseValidator;

    /// <summary>
    /// Initializes a new instance of the <see cref="FileAnalysisStubFactoryOptionsValidator"/> class.
    /// </summary>
    /// <param name="baseValidator">The validator for the base options type.</param>
    public FileAnalysisStubFactoryOptionsValidator(
        IValidateOptions<AnalysisStubFactoryOptions> baseValidator)
    {
        _baseValidator = baseValidator;
    }

    /// <inheritdoc/>
    public ValidateOptionsResult Validate(string? name, FileAnalysisStubFactoryOptions options)
    {
        ValidateOptionsResult baseResult = _baseValidator.Validate(name, options);
        ValidateOptionsResult fileReportsResult = ValidateFileReports(options);

        if (!baseResult.Failed && !fileReportsResult.Failed)
        {
            return ValidateOptionsResult.Success;
        }

        IReadOnlyList<string> allFailures = baseResult.UnionFailureMessages(
            fileReportsResult);
        return ValidateOptionsResult.Fail(allFailures);
    }

    /// <summary>
    /// Validates the file reports configuration in the options.
    /// </summary>
    /// <param name="options">The file analysis stub factory options to validate.</param>
    /// <returns>A <see cref="ValidateOptionsResult"/> indicating success or containing validation failures.</returns>
    private static ValidateOptionsResult ValidateFileReports(
        FileAnalysisStubFactoryOptions options)
    {
         string[] verdictFailures = options.ReturnableFileReports
                 .Select(f => f.VerdictSimulation.Validate())
                 .Where(r => r.Failed)
                 .SelectMany(r => r.Failures ?? [])
                 .ToArray();

         string[] threatScoreFailures = options.ReturnableFileReports
             .Select(f => f.ThreatScoreSimulation.Validate())
             .Where(r => r.Failed)
             .SelectMany(r => r.Failures ?? [])
             .ToArray();

         string[] allFailures = verdictFailures.Union(threatScoreFailures).ToArray();
         return allFailures.Length > 0
             ? ValidateOptionsResult.Fail(allFailures)
             : ValidateOptionsResult.Success;
    }
}
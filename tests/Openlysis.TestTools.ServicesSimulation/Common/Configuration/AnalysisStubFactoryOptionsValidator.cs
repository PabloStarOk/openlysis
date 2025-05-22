using Microsoft.Extensions.Options;

using Openlysis.TestTools.ServicesSimulation.Common.Extensions;

namespace Openlysis.TestTools.ServicesSimulation.Common.Configuration;

/// <summary>
/// Validates the configuration options for the <see cref="AnalysisStubFactoryOptions"/>.
/// Ensures that all required settings are properly configured and valid.
/// </summary>
internal sealed class AnalysisStubFactoryOptionsValidator
    : IValidateOptions<AnalysisStubFactoryOptions>
{
    /// <inheritdoc/>
    public ValidateOptionsResult Validate(
        string? name,
        AnalysisStubFactoryOptions options)
    {
        ValidateOptionsResult verdictResult = options.VerdictSimulation.Validate();
        ValidateOptionsResult statusResult = options.StatusSimulation.Validate();
        ValidateOptionsResult threatScore =
            options.ThreatScoreSimulation.Validate();

        if (!verdictResult.Failed && !statusResult.Failed && !threatScore.Failed)
        {
            return ValidateOptionsResult.Success;
        }

        IEnumerable<string> failureMessages = verdictResult.UnionFailureMessages(
            statusResult,
            threatScore);
        return ValidateOptionsResult.Fail(failureMessages);
    }
}
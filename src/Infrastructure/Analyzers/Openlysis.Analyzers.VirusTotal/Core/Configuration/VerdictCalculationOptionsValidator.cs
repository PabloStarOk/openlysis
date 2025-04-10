using Microsoft.Extensions.Options;

namespace Openlysis.Analyzers.VirusTotal.Core.Configuration;

/// <summary>
/// Validates the <see cref="VerdictCalculationOptions"/> to ensure the sum of threshold percentages is 100.
/// </summary>
public class VerdictCalculationOptionsValidator : IValidateOptions<VerdictCalculationOptions>
{
    /// <inheritdoc/>
    public ValidateOptionsResult Validate(string? name, VerdictCalculationOptions options)
    {
        float totalPercentage = options.MaliciousThresholdPercentage
            + options.SuspiciousThresholdPercentage
            + options.UndetectedThresholdPercentage
            + options.MaxAllowedTimeoutPercentage;

        return Math.Abs(totalPercentage - 100) > 0
            ? ValidateOptionsResult.Fail($"The sum of threshold percentages must be 100, but was {totalPercentage}.")
            : ValidateOptionsResult.Success;
    }
}
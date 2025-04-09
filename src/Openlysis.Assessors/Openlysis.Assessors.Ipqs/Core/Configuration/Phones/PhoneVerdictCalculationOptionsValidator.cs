using Microsoft.Extensions.Options;

namespace Openlysis.Assessors.Ipqs.Core.Configuration.Phones;

/// <summary>
/// Validator for <see cref="PhoneVerdictCalculationOptions"/>.
/// Ensures that the fraud score suspicious threshold is not higher than the fraud score malicious threshold.
/// </summary>
public class PhoneVerdictCalculationOptionsValidator : IValidateOptions<PhoneVerdictCalculationOptions>
{
    /// <inheritdoc/>
    public ValidateOptionsResult Validate(
        string? name,
        PhoneVerdictCalculationOptions options)
    {
        return options.FraudScoreSuspiciousThreshold
            >= options.FraudScoreMaliciousThreshold
                ? ValidateOptionsResult.Fail("Value of FraudScoreSuspiciousThreshold cannot be higher or equals than FraudScoreMaliciousThreshold")
                : ValidateOptionsResult.Success;
    }
}
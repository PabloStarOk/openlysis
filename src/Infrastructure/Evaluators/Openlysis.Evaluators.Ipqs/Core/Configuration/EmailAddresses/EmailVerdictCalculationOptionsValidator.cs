using Microsoft.Extensions.Options;

namespace Openlysis.Evaluators.Ipqs.Core.Configuration.EmailAddresses;

/// <summary>
/// Validator for <see cref="EmailVerdictCalculationOptions"/> to ensure configuration values are valid.
/// </summary>
internal class EmailVerdictCalculationOptionsValidator
    : IValidateOptions<EmailVerdictCalculationOptions>
{
    /// <inheritdoc/>
    public ValidateOptionsResult Validate(
        string? name,
        EmailVerdictCalculationOptions options)
    {
        return options.FraudScoreSuspiciousThreshold
            >= options.FraudScoreMaliciousThreshold
                ? ValidateOptionsResult.Fail("Value of FraudScoreSuspiciousThreshold cannot be higher or equals than FraudScoreMaliciousThreshold")
                : ValidateOptionsResult.Success;
    }
}
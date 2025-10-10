using Microsoft.Extensions.Options;

using Openlysis.Domain.Common.Enums;
using Openlysis.Evaluators.Ipqs.Core.Configuration.Phones;
using Openlysis.Evaluators.Ipqs.Core.Models.Responses;
using Openlysis.Evaluators.Shared.Contracts.Abstractions;

namespace Openlysis.Evaluators.Ipqs.Adapters.Phones;

/// <summary>
/// A calculator for determining the verdict of a phone number based on various criteria.
/// </summary>
public class PhoneVerdictCalculator : IVerdictCalculator<ValidatePhoneResponse>
{
    private readonly IOptionsSnapshot<PhoneVerdictCalculationOptions> _options;

    /// <summary>
    /// Initializes a new instance of the <see cref="PhoneVerdictCalculator"/> class.
    /// </summary>
    /// <param name="options">The options snapshot containing the configuration for phone verdict calculation.</param>
    public PhoneVerdictCalculator(
        IOptionsSnapshot<PhoneVerdictCalculationOptions> options)
    {
        _options = options;
    }

    /// <inheritdoc/>
    public Verdict Calculate(ValidatePhoneResponse input)
    {
        if (!input.IsValid)
        {
            return Verdict.Malicious;
        }

        if (input.RecentAbuse == true || input.IsRisky == true)
        {
            return Verdict.Malicious;
        }

        PhoneVerdictCalculationOptions options = _options.Value;
        var verdict = GetBaseVerdict(input.FraudScore);
        verdict = EscalateIfTrue(verdict, options.SpamVerdict, input.IsSpammer);
        verdict = EscalateIfTrue(verdict, options.VoipVerdict, input.IsVoip);
        verdict = EscalateIfTrue(verdict, options.InactiveVerdict, !input.IsActive);
        return verdict;
    }

    /// <summary>
    /// Escalates the current verdict to the target verdict if the specified condition is true.
    /// </summary>
    /// <param name="currentVerdict">The current verdict.</param>
    /// <param name="targetVerdict">The target verdict to escalate to if the condition is met.</param>
    /// <param name="value">The condition to check; if true, the verdict is escalated.</param>
    /// <returns>The escalated verdict if the condition is true; otherwise, the current verdict.</returns>
    private static Verdict EscalateIfTrue(Verdict currentVerdict, Verdict targetVerdict, bool? value)
    {
        if (value == true
            && targetVerdict > currentVerdict)
        {
            return targetVerdict;
        }

        return currentVerdict;
    }

    /// <summary>
    /// Determines the base verdict based on the provided fraud score.
    /// </summary>
    /// <param name="fraudScore">The fraud score to evaluate.</param>
    /// <returns>The base verdict.</returns>
    private Verdict GetBaseVerdict(int fraudScore)
    {
        if (fraudScore >= _options.Value.FraudScoreMaliciousThreshold)
        {
            return Verdict.Malicious;
        }

        return fraudScore >= _options.Value.FraudScoreSuspiciousThreshold
            ? Verdict.Suspicious
            : Verdict.Undetected;
    }
}
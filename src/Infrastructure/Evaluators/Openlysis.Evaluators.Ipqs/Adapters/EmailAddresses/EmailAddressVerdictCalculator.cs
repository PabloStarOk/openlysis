using Microsoft.Extensions.Options;

using Openlysis.Domain.Common.Enums;
using Openlysis.Evaluators.Ipqs.Core.Configuration.EmailAddresses;
using Openlysis.Evaluators.Ipqs.Core.Models.Enums;
using Openlysis.Evaluators.Ipqs.Core.Models.Responses;
using Openlysis.Evaluators.Shared.Contracts.Abstractions;

namespace Openlysis.Evaluators.Ipqs.Adapters.EmailAddresses;

/// <summary>
/// Determines the verdict of a reputation for an email address.
/// </summary>
internal class EmailAddressVerdictCalculator : IVerdictCalculator<VerifyEmailAddressResponse>
{
    private readonly IOptionsSnapshot<EmailVerdictCalculationOptions> _options;
    private Verdict _currentVerdict = Verdict.Unknown;

    /// <summary>
    /// Initializes a new instance of the <see cref="EmailAddressVerdictCalculator"/> class.
    /// </summary>
    /// <param name="options">
    /// The configuration options for email verdict calculation, provided as an <see cref="IOptionsSnapshot{TOptions}"/>.
    /// </param>
    public EmailAddressVerdictCalculator(
        IOptionsSnapshot<EmailVerdictCalculationOptions> options)
    {
        _options = options;
    }

    /// <inheritdoc/>
    public Verdict Calculate(VerifyEmailAddressResponse input)
    {
        if (input is { IsTimedOut: true, IsValid: false })
        {
            return Verdict.Unknown;
        }

        if (!input.IsValid)
        {
            return Verdict.Suspicious;
        }

        _currentVerdict = Verdict.Undetected;

        EscalateIfTrue(
            Verdict.Suspicious,
            input.IsDisposable || input.IsRiskyTld);

        EscalateIfTrue(
            Verdict.Malicious,
            input is { IsDisposable: true, IsRiskyTld: true });

        EscalateIfTrue(
            Verdict.Suspicious,
            !input.HasProperDmarcDnsRecord || !input.HasProperSpfDnsRecord);

        EscalateIfTrue(
            Verdict.Suspicious,
            input.DomainTrust is DomainTrustType.Suspicious);

        EscalateIfTrue(
            Verdict.Malicious,
            input.DomainTrust is DomainTrustType.Malicious);

        EscalateIfTrue(
            Verdict.Suspicious,
            input.FraudScore >= _options.Value.FraudScoreSuspiciousThreshold);
        EscalateIfTrue(
            Verdict.Malicious,
            input.FraudScore >= _options.Value.FraudScoreMaliciousThreshold);

        EscalateIfTrue(Verdict.Suspicious, input.OverallScore < 2);

        return _currentVerdict;
    }

    /// <summary>
    /// Escalates the current verdict to the specified target verdict if the given condition is true
    /// and the target verdict has a higher severity than the current verdict.
    /// </summary>
    /// <param name="targetVerdict">The verdict to escalate to if the condition is met.</param>
    /// <param name="value">The condition that determines whether to escalate the verdict.</param>
    private void EscalateIfTrue(Verdict targetVerdict, bool? value)
    {
        if (value == true
            && targetVerdict > _currentVerdict)
        {
            _currentVerdict = targetVerdict;
        }
    }
}
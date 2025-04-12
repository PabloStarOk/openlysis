using Microsoft.Extensions.Options;

using Openlysis.Application.Phones.Contracts.Requests;
using Openlysis.Evaluators.Ipqs.Core.Configuration.Common;
using Openlysis.Evaluators.Ipqs.Core.Constants;
using Openlysis.Evaluators.Shared.Contracts.Abstractions;

namespace Openlysis.Evaluators.Ipqs.Adapters.Phones;

/// <summary>
/// Factory class for creating endpoint address to validate a phone number using IPQualityScore service.
/// </summary>
/// <seealso cref="IEndpointAddressFactory{EvaluatePhoneReputation}"/>
public class PhoneEndpointAddressFactory : IEndpointAddressFactory<EvaluatePhoneReputation>
{
    private readonly IOptionsSnapshot<IpqsSecretOptions> _secretOptions;
    private readonly IOptionsSnapshot<IpqsEvaluatorOptions> _evaluatorOptions;

    /// <summary>
    /// Initializes a new instance of the <see cref="PhoneEndpointAddressFactory"/> class.
    /// </summary>
    /// <param name="secretOptions">The options snapshot for IPQS secret options.</param>
    /// <param name="evaluatorOptions">The options snapshot for IPQS evaluator options.</param>
    public PhoneEndpointAddressFactory(
        IOptionsSnapshot<IpqsSecretOptions> secretOptions,
        IOptionsSnapshot<IpqsEvaluatorOptions> evaluatorOptions)
    {
        _secretOptions = secretOptions;
        _evaluatorOptions = evaluatorOptions;
    }

    /// <inheritdoc/>
    public Uri Create(EvaluatePhoneReputation data)
    {
        string formattedUrl = string.Format(
            Addresses.PhoneNumberValidation,
            _secretOptions.Value.ApiKey,
            data.Value);

        return new Uri(
            _evaluatorOptions.Value.BaseAddress,
            formattedUrl);
    }
}
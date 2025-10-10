using System.Net.Mail;

using Microsoft.Extensions.Options;

using Openlysis.Evaluators.Ipqs.Core.Configuration.Common;
using Openlysis.Evaluators.Ipqs.Core.Configuration.EmailAddresses;
using Openlysis.Evaluators.Ipqs.Core.Constants;
using Openlysis.Evaluators.Shared.Contracts.Abstractions;
using Openlysis.Infrastructure.Shared.Infrastructure.Secrets;

namespace Openlysis.Evaluators.Ipqs.Adapters.Common;

/// <summary>
/// Factory class for creating endpoint address to validate a phone number using IPQualityScore service.
/// </summary>
/// <seealso cref="IEndpointAddressFactory{EvaluatePhoneReputation}"/>
internal class EndpointAddressFactory
    : IEndpointAddressFactory<MailAddress>,
      IEndpointAddressFactory<string>
{
    private readonly IOptionsSnapshot<IpqsEvaluatorOptions> _evaluatorOptions;
    private readonly IOptionsSnapshot<EmailAddressVerificationOptions> _emailOptions;
    private readonly IApiKeyProvider _apiKeyProvider;

    /// <summary>
    /// Initializes a new instance of the <see cref="EndpointAddressFactory"/> class.
    /// </summary>
    /// <param name="evaluatorOptions">
    /// The options snapshot containing the base address and other evaluator configurations for IPQS.
    /// </param>
    /// <param name="emailOptions">
    /// The options snapshot containing email verification-specific configurations, such as timeout settings.
    /// </param>
    /// <param name="apiKeyProvider">
    /// The API key provider instance. Add new params here, e\.g\. <c>string apiVersion</c>, <c>bool useCache</c>.
    /// </param>
    public EndpointAddressFactory(
        IOptionsSnapshot<IpqsEvaluatorOptions> evaluatorOptions,
        IOptionsSnapshot<EmailAddressVerificationOptions> emailOptions,
        IApiKeyProvider apiKeyProvider)
    {
        _evaluatorOptions = evaluatorOptions;
        _emailOptions = emailOptions;
        _apiKeyProvider = apiKeyProvider;
    }

    /// <inheritdoc/>
    public Uri Create(string data)
    {
        return CreateWith(
            Addresses.PhoneNumberValidation,
            data);
    }

    /// <inheritdoc/>
    public Uri Create(MailAddress data)
    {
        Uri baseUri = CreateWith(
            Addresses.EmailAddressVerification,
            data.Address);

        var uriBuilder = new UriBuilder(baseUri)
        {
            Query = $"timeout={_emailOptions.Value.Timeout}",
        };

        return uriBuilder.Uri;
    }

    /// <summary>
    /// Creates a URI by formatting the provided address with the API key and value,
    /// and combining it with the base address from the evaluator options.
    /// </summary>
    /// <param name="address">The endpoint address template to format.</param>
    /// <param name="value">The value to include in the formatted address.</param>
    /// <returns>A fully constructed <see cref="Uri"/> object.</returns>
    private Uri CreateWith(
        string address,
        string value)
    {
        string formattedUrl = string.Format(
            address,
            _apiKeyProvider.GetApiKey(_evaluatorOptions.Value.ApiKeySecretName),
            value);

        return new Uri(
            _evaluatorOptions.Value.BaseAddress,
            formattedUrl);
    }
}
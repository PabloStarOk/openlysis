using System.Net.Mail;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

using Openlysis.Domain.EmailAddresses.Entities;
using Openlysis.Evaluators.Ipqs.Core.Configuration.Common;
using Openlysis.Evaluators.Ipqs.Core.Constants;
using Openlysis.Evaluators.Shared.Contracts.Abstractions;
using Openlysis.Evaluators.Shared.Infrastructure.RateQuota.Enums;
using Openlysis.Infrastructure.Shared.Contracts.Common.Abstractions;
using Openlysis.Infrastructure.Shared.Infrastructure.RateQuota.Abstractions;

namespace Openlysis.Evaluators.Ipqs.Adapters.EmailAddresses;

/// <summary>
/// Evaluates the reputation of email addresses using the IPQS service.
/// </summary>
public class EmailAddressReputationEvaluator
    : ReputationEvaluator<MailAddress, EmailAddressReputation>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="EmailAddressReputationEvaluator"/> class.
    /// </summary>
    /// <param name="logger">The logger instance for logging evaluator activities.</param>
    /// <param name="options">The options monitor for reputation evaluator configuration.</param>
    /// <param name="httpClientFactory">The factory for creating HTTP client instances.</param>
    /// <param name="rateQuotaService">The rate quota service for managing request quotas, keyed by <see cref="KeyedServices.GlobalKey"/>.</param>
    /// <param name="endpointAddressFactory">The factory for creating endpoint addresses, keyed by <see cref="KeyedServices.EmailAddressKey"/>.</param>
    /// <param name="responseParser">The parser for processing responses, keyed by <see cref="KeyedServices.EmailAddressKey"/>.</param>
    public EmailAddressReputationEvaluator(
        IServiceLogger<EmailAddressReputationEvaluator> logger,
        IOptionsMonitor<IpqsEvaluatorOptions> options,
        IHttpClientFactory httpClientFactory,
        [FromKeyedServices(KeyedServices.GlobalKey)] IRateQuotaService<ReputationEndpointType> rateQuotaService,
        [FromKeyedServices(KeyedServices.EmailAddressKey)] IEndpointAddressFactory<MailAddress> endpointAddressFactory,
        [FromKeyedServices(KeyedServices.EmailAddressKey)] IResponseParser<EmailAddressReputation> responseParser)
        : base(logger, options, httpClientFactory, rateQuotaService, endpointAddressFactory, responseParser)
    {
    }
}
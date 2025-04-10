using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

using Openlysis.Application.Phones.Requests;
using Openlysis.Domain.Phones.Entities;
using Openlysis.Evaluators.Ipqs.Core.Configuration.Common;
using Openlysis.Evaluators.Ipqs.Core.Constants;
using Openlysis.Evaluators.Shared.Contracts.Abstractions;
using Openlysis.Evaluators.Shared.Infrastructure.Logging.Services;
using Openlysis.Evaluators.Shared.Infrastructure.RateQuota.Enums;
using Openlysis.Infrastructure.Shared.Infrastructure.RateQuota.Abstractions;

namespace Openlysis.Evaluators.Ipqs.Adapters.Phones;

/// <summary>
/// Evaluates the reputation of phone numbers using the IPQS service.
/// </summary>
public class PhoneReputationEvaluator
    : ReputationEvaluator<EvaluatePhoneReputation, PhoneServiceReputation>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="PhoneReputationEvaluator"/> class.
    /// </summary>
    /// <param name="logger">The logger instance for logging.</param>
    /// <param name="options">The options monitor for data evaluator options.</param>
    /// <param name="httpClientFactory">The HTTP client factory for creating HTTP clients.</param>
    /// <param name="rateQuotaService">The rate quota service for managing rate limits.</param>
    /// <param name="endpointAddressFactory">The endpoint address factory for creating endpoint addresses.</param>
    /// <param name="responseParser">The response parser for parsing service responses.</param>
    public PhoneReputationEvaluator(
        ReputationEvaluatorLogger<PhoneReputationEvaluator> logger,
        IOptionsMonitor<IpqsEvaluatorOptions> options,
        IHttpClientFactory httpClientFactory,
        [FromKeyedServices(KeyedServices.GlobalKey)] IRateQuotaService<ReputationEndpointType> rateQuotaService,
        [FromKeyedServices(KeyedServices.PhoneKey)] IEndpointAddressFactory<EvaluatePhoneReputation> endpointAddressFactory,
        [FromKeyedServices(KeyedServices.PhoneKey)] IResponseParser<PhoneServiceReputation> responseParser)
        : base(logger, options, httpClientFactory, rateQuotaService, endpointAddressFactory, responseParser)
    {
    }
}
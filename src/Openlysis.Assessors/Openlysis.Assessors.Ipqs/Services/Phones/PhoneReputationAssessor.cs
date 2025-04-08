using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

using Openlysis.Application.Phones.Requests;
using Openlysis.Assessors.Ipqs.Core.Configuration.Common;
using Openlysis.Assessors.Ipqs.Core.Constants;
using Openlysis.Assessors.Shared.Abstractions;
using Openlysis.Assessors.Shared.Infrastructure.Logging.Services;
using Openlysis.Assessors.Shared.Infrastructure.RateQuota.Enums;
using Openlysis.Domain.Phones.Entities;
using Openlysis.Infrastructure.Shared.RateQuota.Abstractions;

namespace Openlysis.Assessors.Ipqs.Services.Phones;

/// <summary>
/// Assesses the reputation of phone numbers using the IPQS service.
/// </summary>
public class PhoneReputationAssessor
    : ReputationAssessor<AssessPhoneNumber, PhoneServiceReputation>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="PhoneReputationAssessor"/> class.
    /// </summary>
    /// <param name="logger">The logger instance for logging.</param>
    /// <param name="options">The options monitor for data assessor options.</param>
    /// <param name="httpClientFactory">The HTTP client factory for creating HTTP clients.</param>
    /// <param name="rateQuotaService">The rate quota service for managing rate limits.</param>
    /// <param name="endpointAddressFactory">The endpoint address factory for creating endpoint addresses.</param>
    /// <param name="responseParser">The response parser for parsing service responses.</param>
    public PhoneReputationAssessor(
        AssessorLogger<IpqsAssessorOptions> logger,
        IOptionsMonitor<IpqsAssessorOptions> options,
        IHttpClientFactory httpClientFactory,
        [FromKeyedServices(KeyedServices.GlobalKey)] IRateQuotaService<AssessorEndpointType> rateQuotaService,
        [FromKeyedServices(KeyedServices.PhoneKey)] IEndpointAddressFactory<AssessPhoneNumber> endpointAddressFactory,
        [FromKeyedServices(KeyedServices.PhoneKey)] IResponseParser<PhoneServiceReputation> responseParser)
        : base(logger, options, httpClientFactory, rateQuotaService, endpointAddressFactory, responseParser)
    {
    }
}
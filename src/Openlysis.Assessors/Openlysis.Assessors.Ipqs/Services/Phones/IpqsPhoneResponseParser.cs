using ErrorOr;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

using Openlysis.Assessors.Ipqs.Core.Configuration.Common;
using Openlysis.Assessors.Ipqs.Core.Constants;
using Openlysis.Assessors.Ipqs.Core.Models;
using Openlysis.Assessors.Shared.Abstractions;
using Openlysis.Domain.Common.Enums;
using Openlysis.Domain.Phones.Entities;
using Openlysis.Domain.Phones.ValueObjects;
using Openlysis.Infrastructure.Shared.Deserialization.Abstractions;

namespace Openlysis.Assessors.Ipqs.Services.Phones;

/// <summary>
/// Parses the response from the IPQS phone service and converts it into a <see cref="PhoneServiceReputation"/> object.
/// </summary>
public class IpqsPhoneResponseParser : IResponseParser<PhoneServiceReputation>
{
    private readonly ILogger<IpqsPhoneResponseParser> _logger;
    private readonly IOptionsSnapshot<IpqsAssessorOptions> _assessorOptions;
    private readonly IServiceDeserializer _serviceDeserializer;
    private readonly IVerdictCalculator<ValidatePhoneResponse> _verdictCalculator;

    /// <summary>
    /// Initializes a new instance of the <see cref="IpqsPhoneResponseParser"/> class.
    /// </summary>
    /// <param name="logger">The logger instance for logging information.</param>
    /// <param name="assessorOptions">The options snapshot for IPQS assessor configuration.</param>
    /// <param name="serviceDeserializer">The service deserializer used to deserialize the response.</param>
    /// <param name="verdictCalculator">The verdict calculator used to calculate the verdict from the phone response.</param>
    public IpqsPhoneResponseParser(
        ILogger<IpqsPhoneResponseParser> logger,
        IOptionsSnapshot<IpqsAssessorOptions> assessorOptions,
        [FromKeyedServices(KeyedServices.PhoneKey)] IServiceDeserializer serviceDeserializer,
        IVerdictCalculator<ValidatePhoneResponse> verdictCalculator)
    {
        _logger = logger;
        _assessorOptions = assessorOptions;
        _serviceDeserializer = serviceDeserializer;
        _verdictCalculator = verdictCalculator;
    }

    /// <inheritdoc/>
    public async Task<ErrorOr<PhoneServiceReputation>> ParseAsync(
        HttpResponseMessage response,
        CancellationToken cancellationToken = default)
    {
#if DEBUG
        _logger.LogDebug(
            "{ServiceName}: Phone number validation response: {Response}",
            _assessorOptions.Value.ServiceName,
            await response.Content.ReadAsStringAsync(cancellationToken));
#endif

        ErrorOr<ValidatePhoneResponse> deserializationResult = await _serviceDeserializer
            .DeserializeAsync<ValidatePhoneResponse>(response, cancellationToken);

        if (deserializationResult.IsError)
        {
            return deserializationResult.Errors;
        }

        ValidatePhoneResponse phoneResponse = deserializationResult.Value;
        if (!phoneResponse.Success)
        {
            _logger.LogError(
                "{ServiceName}: ValidatePhoneResponse was not success. \n\tMessage: {Message}\n\tErrors: {Errors}",
                _assessorOptions.Value.ServiceName,
                phoneResponse.Message,
                phoneResponse.Errors);
            return Error.Failure("Request was not success.");
        }

        var phoneInfo = new PhoneInfo(
            phoneResponse.LocalFormat,
            phoneResponse.CountryCode,
            phoneResponse.DialingCode ?? 0,
            phoneResponse.LineType);

        Verdict verdict = _verdictCalculator.Calculate(phoneResponse);

        return PhoneServiceReputation.Create(
            _assessorOptions.Value.ServiceName,
            verdict,
            phoneInfo);
    }
}
using ErrorOr;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

using Openlysis.Domain.Common.Enums;
using Openlysis.Domain.EmailAddresses.Entities;
using Openlysis.Evaluators.Ipqs.Core.Configuration.Common;
using Openlysis.Evaluators.Ipqs.Core.Constants;
using Openlysis.Evaluators.Ipqs.Core.Models.Responses;
using Openlysis.Evaluators.Shared.Contracts.Abstractions;
using Openlysis.Infrastructure.Shared.Infrastructure.Deserialization.Abstractions;

namespace Openlysis.Evaluators.Ipqs.Adapters.EmailAddresses;

/// <summary>
/// Parses HTTP responses to extract email address service reputation data.
/// </summary>
/// <remarks>
/// Implements the <see cref="IResponseParser{T}"/> interface for parsing
/// <see cref="EmailAddressReputation"/> objects.
/// </remarks>
internal class EmailAddressResponseParser : IResponseParser<EmailAddressReputation>
{
    private readonly ILogger<EmailAddressResponseParser> _logger;
    private readonly IOptionsSnapshot<IpqsEvaluatorOptions> _evaluatorOptions;
    private readonly IServiceDeserializer _serviceDeserializer;
    private readonly IVerdictCalculator<VerifyEmailAddressResponse> _verdictCalculator;

    /// <summary>
    /// Initializes a new instance of the <see cref="EmailAddressResponseParser"/> class.
    /// </summary>
    /// <param name="logger">
    /// The logger instance used for logging debug and error information related to email address parsing.
    /// </param>
    /// <param name="evaluatorOptions">
    /// The evaluator options containing configuration values for the IPQS evaluator.
    /// </param>
    /// <param name="serviceDeserializer">
    /// The service deserializer used to deserialize HTTP responses. This is injected as a keyed service
    /// with the key <see cref="KeyedServices.EmailAddressKey"/>.
    /// </param>
    /// <param name="verdictCalculator">
    /// The verdict calculator used to compute the final verdict for the email address reputation.
    /// </param>
    public EmailAddressResponseParser(
        ILogger<EmailAddressResponseParser> logger,
        IOptionsSnapshot<IpqsEvaluatorOptions> evaluatorOptions,
        [FromKeyedServices(KeyedServices.EmailAddressKey)] IServiceDeserializer serviceDeserializer,
        IVerdictCalculator<VerifyEmailAddressResponse> verdictCalculator)
    {
        _logger = logger;
        _evaluatorOptions = evaluatorOptions;
        _serviceDeserializer = serviceDeserializer;
        _verdictCalculator = verdictCalculator;
    }

    /// <inheritdoc/>
    public async Task<ErrorOr<EmailAddressReputation>> ParseAsync(
        HttpResponseMessage response,
        CancellationToken cancellationToken = default)
    {
#if DEBUG
        _logger.LogDebug(
            "{ServiceName}: Email address number verification response: {Response}",
            _evaluatorOptions.Value.ServiceName,
            await response.Content.ReadAsStringAsync(cancellationToken));
#endif

        ErrorOr<VerifyEmailAddressResponse> deserializationResult = await _serviceDeserializer
            .DeserializeAsync<VerifyEmailAddressResponse>(response, cancellationToken);

        if (deserializationResult.IsError)
        {
            return deserializationResult.Errors;
        }

        VerifyEmailAddressResponse emailResponse = deserializationResult.Value;
        if (!emailResponse.Success)
        {
            _logger.LogError(
                "{ServiceName}: {Type} was not success. \n\tMessage: {Message}\n\tErrors: {Errors}",
                _evaluatorOptions.Value.ServiceName,
                typeof(VerifyEmailAddressResponse),
                emailResponse.Message,
                emailResponse.Errors);
            return Error.Failure("Request was not success.");
        }

        if (emailResponse.IsTimedOut)
        {
            _logger.LogWarning(
                "{ServiceName}: {Type} timed out which increase likelihood of being a false verdict.\n\tMessage: {Message}",
                _evaluatorOptions.Value.ServiceName,
                typeof(VerifyEmailAddressResponse),
                emailResponse.Message);
        }

        Verdict verdict = _verdictCalculator.Calculate(emailResponse);
        return EmailAddressReputation.Create(
            _evaluatorOptions.Value.ServiceName,
            verdict,
            emailResponse.IsDisposable,
            emailResponse.IsRiskyTld);
    }
}
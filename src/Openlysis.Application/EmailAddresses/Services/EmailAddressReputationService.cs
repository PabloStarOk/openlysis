using ErrorOr;

using Microsoft.Extensions.Logging;

using Openlysis.Application.Common.Abstractions.Contracts;
using Openlysis.Application.EmailAddresses.Contracts.Requests;
using Openlysis.Domain.EmailAddresses;
using Openlysis.Domain.EmailAddresses.Entities;

namespace Openlysis.Application.EmailAddresses.Services;

// TODO: Refactor this service to use a generic one for phone numbers and email addresses.

/// <summary>
/// Service responsible for getting the reputation of email addresses
/// by using multiple reputation evaluators.
/// </summary>
internal class EmailAddressReputationService : IEmailAddressReputationService
{
    private readonly ILogger<EmailAddressReputationService> _logger;
    private readonly TimeProvider _timeProvider;
    private readonly IEnumerable<IReputationEvaluator<EvaluateEmailAddressReputation, EmailAddressServiceReputation>> _reputationEvaluators;

    /// <summary>
    /// Initializes a new instance of the <see cref="EmailAddressReputationService"/> class.
    /// </summary>
    /// <param name="logger">The logger instance for logging information and errors.</param>
    /// <param name="timeProvider">The time provider for retrieving the current UTC time.</param>
    /// <param name="reputationEvaluators">
    /// A collection of reputation evaluators used to evaluate email address reputations.
    /// </param>
    public EmailAddressReputationService(
        ILogger<EmailAddressReputationService> logger,
        TimeProvider timeProvider,
        IEnumerable<IReputationEvaluator<EvaluateEmailAddressReputation, EmailAddressServiceReputation>> reputationEvaluators)
    {
        _logger = logger;
        _timeProvider = timeProvider;
        _reputationEvaluators = reputationEvaluators;
    }

    /// <inheritdoc/>
    public bool IsAvailable => _reputationEvaluators.Any(r => r.IsAvailable);

    /// <inheritdoc/>
    public async Task<ErrorOr<EmailAddressMultiReputation>> GetAsync(
        EvaluateEmailAddressReputation evaluateEmailAddressReputation,
        CancellationToken cancellationToken = default)
    {
        if (!IsAvailable)
        {
            Error.Failure("Service is not available.");
        }

        List<Error> errors = [];
        var multiReputation = EmailAddressMultiReputation.Create(
            _timeProvider.GetUtcNow().UtcDateTime,
            evaluateEmailAddressReputation.Value);

        // TODO: When service is unavailable, wait a timeout along with an event from the service.
        await Parallel.ForEachAsync(_reputationEvaluators, cancellationToken, async (evaluator, ct) =>
        {
            ErrorOr<EmailAddressServiceReputation> result = await evaluator
                .EvaluateAsync(evaluateEmailAddressReputation, ct);

            if (result.IsError)
            {
                errors.AddRange(result.Errors);
                _logger.LogError(
                    "Error received from {EvaluatorName}.\n\tIs Service Available: {IsAvailable}\n\tErrors: {Errors}",
                    evaluator.ServiceName,
                    evaluator.IsAvailable,
                    errors);
                return;
            }

            multiReputation.AddServiceReputation(result.Value);
        });

        if (multiReputation.ServicesReputations.Count > 0)
        {
            return multiReputation;
        }

        if (errors.Count > 0)
        {
            return errors;
        }

        string errorMessage = $"{typeof(EmailAddressMultiReputation)} object doesn't contain {typeof(EmailAddressServiceReputation)} objects and none error was returned by evaluator services.";
        _logger.LogError(errorMessage);
        return Error.Unexpected(description: errorMessage);
    }
}
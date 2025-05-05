using ErrorOr;

using Microsoft.Extensions.Logging;

using Openlysis.Application.Common.Abstractions.Contracts;
using Openlysis.Application.Common.Abstractions.Persistence;
using Openlysis.Application.Phones.Contracts.Requests;
using Openlysis.Domain.Common.ValueObjects;
using Openlysis.Domain.Phones;
using Openlysis.Domain.Phones.Entities;

namespace Openlysis.Application.Phones.Services;

/// <summary>
/// Service responsible for evaluating and managing phone reputations.
/// </summary>
/// <remarks>
/// This service uses multiple reputation evaluators to get phone reputations
/// and provides functionality to store the results in a database.
/// </remarks>
internal class PhoneReputationService : IPhoneReputationService
{
    /// <inheritdoc/>
    public bool IsAvailable => _reputationEvaluators.Any(r => r.IsAvailable);

    private readonly ILogger<PhoneReputationService> _logger;
    private readonly IRepository<PhoneMultiReputation, GlobalId> _repository;
    private readonly TimeProvider _timeProvider;
    private readonly IEnumerable<IReputationEvaluator<EvaluatePhoneReputation, PhoneServiceReputation>> _reputationEvaluators;

    /// <summary>
    /// Initializes a new instance of the <see cref="PhoneReputationService"/> class.
    /// </summary>
    /// <param name="logger">The logger instance for logging information and errors.</param>
    /// <param name="repository">
    /// The repository for storing <see cref="PhoneMultiReputation"/> objects.
    /// </param>
    /// <param name="timeProvider">The time provider to retrieve the current UTC time.</param>
    /// <param name="reputationEvaluators">
    /// A collection of reputation evaluators used to get phone reputations.
    /// </param>
    public PhoneReputationService(
        ILogger<PhoneReputationService> logger,
        IRepository<PhoneMultiReputation, GlobalId> repository,
        TimeProvider timeProvider,
        IEnumerable<IReputationEvaluator<EvaluatePhoneReputation, PhoneServiceReputation>> reputationEvaluators)
    {
        _logger = logger;
        _repository = repository;
        _timeProvider = timeProvider;
        _reputationEvaluators = reputationEvaluators;
    }

    /// <inheritdoc/>
    public async Task<ErrorOr<PhoneMultiReputation>> GetAsync(
        EvaluatePhoneReputation evaluatePhoneReputation,
        bool storeInDatabase,
        CancellationToken cancellationToken = default)
    {
        if (!IsAvailable)
        {
            Error.Failure("Service is not available.");
        }

        List<Error> errors = [];
        var multiReputation = PhoneMultiReputation.Create(
            _timeProvider.GetUtcNow().UtcDateTime,
            evaluatePhoneReputation.Value);

        // TODO: When service is unavailable, wait a timeout along with an event from the service.
        await Parallel.ForEachAsync(_reputationEvaluators, cancellationToken, async (evaluator, ct) =>
        {
            ErrorOr<PhoneServiceReputation> result = await evaluator
                .EvaluateAsync(evaluatePhoneReputation, ct);

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

        if (errors.Count > 0)
        {
            return errors;
        }

        if (multiReputation.ServicesReputations.Count is 0)
        {
            _logger.LogError("PhoneMultiReputation object doesn't contain PhoneServicesReputation objects and none error was returned by an evaluator service.");
            return Error.Unexpected(description: "PhoneMultiReputation object doesn't contain PhoneServicesReputation objects and none error was returned by an evaluator service.");
        }

        if (storeInDatabase)
        {
            await _repository.AddAsync(multiReputation, cancellationToken);
        }

        return multiReputation;
    }
}
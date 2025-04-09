using ErrorOr;

using Microsoft.Extensions.Logging;

using Openlysis.Application.Phones.Interfaces;
using Openlysis.Application.Phones.Requests;
using Openlysis.Domain.Phones;
using Openlysis.Domain.Phones.Entities;

namespace Openlysis.Application.Phones.Services;

/// <summary>
/// Service responsible for assessing the reputation of phone numbers
/// by utilizing multiple reputation assessors.
/// </summary>
internal class PhoneReputationService : IPhoneReputationService
{
    /// <inheritdoc/>
    public bool IsAvailable => _reputationAssessors.Any(r => r.IsAvailable);

    private readonly ILogger<PhoneReputationService> _logger;
    private readonly TimeProvider _timeProvider;
    private readonly IEnumerable<IReputationAssessor<AssessPhoneNumber, PhoneServiceReputation>> _reputationAssessors;

    /// <summary>
    /// Initializes a new instance of the <see cref="PhoneReputationService"/> class.
    /// </summary>
    /// <param name="logger">The logger instance for logging information and errors.</param>
    /// <param name="timeProvider">The time provider to retrieve the current UTC time.</param>
    /// <param name="reputationAssessors">
    /// A collection of reputation assessors used to evaluate phone number reputations.
    /// </param>
    public PhoneReputationService(
        ILogger<PhoneReputationService> logger,
        TimeProvider timeProvider,
        IEnumerable<IReputationAssessor<AssessPhoneNumber, PhoneServiceReputation>> reputationAssessors)
    {
        _logger = logger;
        _timeProvider = timeProvider;
        _reputationAssessors = reputationAssessors;
    }

    /// <inheritdoc/>
    public async Task<ErrorOr<PhoneMultiReputation>> AssessAsync(
        AssessPhoneNumber assessPhoneNumber,
        CancellationToken cancellationToken = default)
    {
        if (!IsAvailable)
        {
            Error.Failure("Service is not available.");
        }

        List<Error> errors = [];
        var multiReputation = PhoneMultiReputation.Create(
            _timeProvider.GetUtcNow().UtcDateTime);

        // TODO: When service is unavailable, wait a timeout along with an event from the service.
        await Parallel.ForEachAsync(_reputationAssessors, cancellationToken, async (assessor, ct) =>
        {
            ErrorOr<PhoneServiceReputation> result = await assessor
                .AssessAsync(assessPhoneNumber, ct);

            if (result.IsError)
            {
                errors.AddRange(result.Errors);
                _logger.LogError(
                    "Error received from {AssessorName}.\n\tIs Service Available: {IsAvailable}\n\tErrors: {Errors}",
                    assessor.ServiceName,
                    assessor.IsAvailable,
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

        _logger.LogError("PhoneMultiReputation object doesn't contain PhoneServicesReputation objects and none error was returned by assessor services.");
        return Error.Unexpected(description: "PhoneMultiReputation object doesn't contain PhoneServicesReputation objects and none error was returned by assessor services.");
    }
}
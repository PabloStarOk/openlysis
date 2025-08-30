using Microsoft.Extensions.Logging;

using Openlysis.Application.Common.Abstractions.Contracts;
using Openlysis.Application.Common.Abstractions.Persistence;
using Openlysis.Application.Reputations;
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
internal class PhoneReputationService :
    DataReputationService<string, PhoneReputation, PhoneMultiReputation>,
    IPhoneReputationService
{
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
        IRepository<PhoneMultiReputation> repository,
        TimeProvider timeProvider,
        IEnumerable<IReputationEvaluator<string, PhoneReputation>> reputationEvaluators)
        : base(logger, repository, timeProvider, reputationEvaluators)
    {
    }

    /// <inheritdoc/>
    protected override PhoneMultiReputation CreateMultiReputation(string data)
    {
        return PhoneMultiReputation.Create(
            TimeProvider.GetUtcNow().UtcDateTime,
            data);
    }
}
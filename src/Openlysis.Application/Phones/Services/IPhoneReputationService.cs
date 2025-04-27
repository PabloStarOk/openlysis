using ErrorOr;

using Openlysis.Application.Phones.Contracts.Requests;
using Openlysis.Domain.Phones;

namespace Openlysis.Application.Phones.Services;

/// <summary>
/// Interface for assessing the reputation of a phone number.
/// </summary>
public interface IPhoneReputationService
{
    /// <summary>
    /// Gets a value indicating whether the service is available for assessing phone reputations.
    /// </summary>
    public bool IsAvailable { get; }

    /// <summary>
    /// Asynchronously assesses the reputation of a phone number.
    /// </summary>
    /// <param name="evaluatePhoneReputation">An object containing the phone number and related details to assess.</param>
    /// <param name="storeInDatabase">A boolean indicating whether to store the multi reputation in the database.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests during the asynchronous operation.</param>
    /// <returns>A task that represents the asynchronous operation, containing the phone number's reputation details as a <see cref="PhoneMultiReputation"/> object.</returns>
    public Task<ErrorOr<PhoneMultiReputation>> AssessAsync(
        EvaluatePhoneReputation evaluatePhoneReputation,
        bool storeInDatabase,
        CancellationToken cancellationToken = default);
}
using ErrorOr;

using Openlysis.Domain.Phones;

namespace Openlysis.Application.Phones.Services;

/// <summary>
/// Defines the contract for a service that evaluates the reputation of phone numbers.
/// </summary>
public interface IPhoneReputationService
{
    /// <summary>
    /// Gets a value indicating whether the phone reputation service is available for use.
    /// </summary>
    public bool IsAvailable { get; }

    /// <summary>
    /// Retrieves the reputation of a phone number based on the provided evaluation request.
    /// </summary>
    /// <param name="phoneNumber">The phone number for which to retrieve reputation details.</param>
    /// <param name="storeInDatabase">Indicates whether the result should be stored in the database.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>
    /// A task that represents the asynchronous operation. The task result contains an
    /// <see cref="ErrorOr{T}"/> object with the phone reputation details or an error.
    /// </returns>
    public Task<ErrorOr<PhoneMultiReputation>> GetAsync(
        string phoneNumber,
        bool storeInDatabase,
        CancellationToken cancellationToken = default);
}
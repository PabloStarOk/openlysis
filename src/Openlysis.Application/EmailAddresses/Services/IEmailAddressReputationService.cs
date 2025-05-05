using System.Net.Mail;

using ErrorOr;

using Openlysis.Domain.EmailAddresses;

namespace Openlysis.Application.EmailAddresses.Services;

/// <summary>
/// Defines a service for evaluating the reputation of email addresses.
/// </summary>
public interface IEmailAddressReputationService
{
    /// <summary>
    /// Gets a value indicating whether the email address reputation service is available.
    /// </summary>
    public bool IsAvailable { get; }

    /// <summary>
    /// Retrieves the reputation details for a given email address.
    /// </summary>
    /// <param name="emailAddress">The email address for which to retrieve reputation details.</param>
    /// <param name="storeInDatabase">A boolean indicating whether to store the multi reputation in the database.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>
    /// A task that represents the asynchronous operation. The task result contains
    /// an <see cref="ErrorOr{T}"/> object with the email address reputation details.
    /// </returns>
    public Task<ErrorOr<EmailAddressMultiReputation>> GetAsync(
        MailAddress emailAddress,
        bool storeInDatabase,
        CancellationToken cancellationToken = default);
}
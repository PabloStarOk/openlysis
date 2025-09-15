using System.Net.Mail;

using Microsoft.Extensions.Logging;

using Openlysis.Application.Common.Abstractions.Contracts;
using Openlysis.Application.Common.Abstractions.Persistence;
using Openlysis.Application.Reputations;
using Openlysis.Domain.EmailAddresses;
using Openlysis.Domain.EmailAddresses.Entities;

namespace Openlysis.Application.EmailAddresses.Services;

/// <summary>
/// Service responsible for getting the reputation of email addresses
/// by using multiple reputation evaluators.
/// </summary>
internal class EmailAddressReputationService :
    DataReputationService<MailAddress, EmailAddressReputation, EmailAddressMultiReputation>,
    IEmailAddressReputationService
{
    /// <summary>
    /// Initializes a new instance of the <see cref="EmailAddressReputationService"/> class.
    /// </summary>
    /// <param name="logger">The logger instance for logging information and errors.</param>
    /// <param name="repository">
    /// The repository for storing <see cref="EmailAddressMultiReputation"/> objects.
    /// </param>
    /// <param name="timeProvider">The time provider for retrieving the current UTC time.</param>
    /// <param name="reputationEvaluators">
    /// A collection of reputation evaluators used to evaluate email address reputations.
    /// </param>
    public EmailAddressReputationService(
        ILogger<EmailAddressReputationService> logger,
        IRepository<EmailAddressMultiReputation> repository,
        TimeProvider timeProvider,
        IEnumerable<IReputationEvaluator<MailAddress, EmailAddressReputation>> reputationEvaluators)
        : base(logger, repository, timeProvider, reputationEvaluators)
    {
    }

    /// <inheritdoc/>
    protected override EmailAddressMultiReputation CreateMultiReputation(MailAddress data)
    {
        return EmailAddressMultiReputation.Create(
            TimeProvider.GetUtcNow(),
            data);
    }
}
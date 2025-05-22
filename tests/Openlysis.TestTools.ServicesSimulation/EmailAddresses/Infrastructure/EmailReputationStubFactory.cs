using Microsoft.Extensions.Logging;

using Openlysis.Domain.Common.Enums;
using Openlysis.Domain.EmailAddresses.Entities;
using Openlysis.TestTools.ServicesSimulation.Common.Services.Abstractions;
using Openlysis.TestTools.ServicesSimulation.EmailAddresses.Configuration;

namespace Openlysis.TestTools.ServicesSimulation.EmailAddresses.Infrastructure;

/// <summary>
/// Factory class for creating email reputation stubs used in testing scenarios.
/// </summary>
/// <remarks>
/// Implements the <see cref="StubFactory{TOptions, TEntity}"/> pattern to generate
/// configurable email address reputation entries for simulation purposes.
/// </remarks>
internal sealed class EmailReputationStubFactory
    : StubFactory<EmailReputationStubFactoryOptions, EmailAddressServiceReputation>
{
    private static readonly IReadOnlyList<bool?> BoolValues = [true, false, null];

    /// <summary>
    /// Initializes a new instance of the <see cref="EmailReputationStubFactory"/> class.
    /// </summary>
    /// <param name="logger">The logger used for recording factory operations.</param>
    public EmailReputationStubFactory(ILogger<EmailReputationStubFactory> logger)
        : base(logger)
    {
    }

    /// <inheritdoc/>
    protected override EmailAddressServiceReputation HandleCreation(
        string serviceName,
        EmailReputationStubFactoryOptions options)
    {
        Verdict verdict = GenerateVerdict(options.VerdictSimulation);

        bool? isDisposable = options.ReturnRandomDisposable
            ? GetRandomValue(BoolValues)
            : options.FixedIsDisposable;

        bool? isRiskyTld = options.ReturnRandomRiskyTld
            ? GetRandomValue(BoolValues)
            : options.FixedIsRiskyTld;

        return EmailAddressServiceReputation.Create(
            serviceName,
            verdict,
            isDisposable,
            isRiskyTld);
    }

    /// <inheritdoc/>
    protected override void LogCreatedStub(EmailAddressServiceReputation stub)
    {
        Logger.LogTrace(
            "{TypeName} created:"
            + "\n\tService name: {ServiceName}"
            + "\n\tID: {Id}"
            + "\n\tVerdict: {Verdict}"
            + "\n\tIs disposable: {IsDisposable}"
            + "\n\tIs risky tld: {IsRiskyTld}",
            nameof(EmailAddressServiceReputation),
            stub.ServiceName,
            stub.Id,
            stub.Verdict,
            stub.IsDisposable.ToString() ?? "null",
            stub.IsRiskyTld.ToString() ?? "null");
    }
}
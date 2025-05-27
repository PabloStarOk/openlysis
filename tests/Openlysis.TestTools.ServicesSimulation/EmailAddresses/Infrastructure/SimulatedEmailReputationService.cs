using System.Net.Mail;

using ErrorOr;

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

using Openlysis.Application.Common.Abstractions.Contracts;
using Openlysis.Domain.EmailAddresses.Entities;
using Openlysis.TestTools.ServicesSimulation.Common.Configuration;
using Openlysis.TestTools.ServicesSimulation.Common.Services.Reputations;
using Openlysis.TestTools.ServicesSimulation.EmailAddresses.Configuration;

namespace Openlysis.TestTools.ServicesSimulation.EmailAddresses.Infrastructure;

/// <summary>
/// Simulates an email address reputation service for testing purposes.
/// This class provides mock implementations of email address reputation operations
/// without making actual service calls.
/// </summary>
internal sealed class SimulatedEmailReputationService :
    IReputationEvaluator<MailAddress, EmailAddressReputation>, IDisposable
{
    /// <inheritdoc/>
    public string ServiceName => _serviceOptions.Name;

    /// <inheritdoc/>
    public bool IsAvailable => _serviceOptions.IsAvailable;

    private readonly ILogger<SimulatedEmailReputationService> _logger;
    private readonly ReputationBehaviorSimulator<EmailAddressReputation, EmailReputationStubFactoryOptions> _behaviorSimulator;
    private readonly string _optionsName;
    private readonly IDisposable? _optionsObserver;
    private ReputationServiceOptions<EmailReputationStubFactoryOptions> _serviceOptions;
    private bool _disposed;

    /// <summary>
    /// Initializes a new instance of the <see cref="SimulatedEmailReputationService"/> class.
    /// </summary>
    /// <param name="logger">The logger for recording diagnostic information.</param>
    /// <param name="optionsName">The name of the configuration options to retrieve.</param>
    /// <param name="optionsMonitor">Provides access to the service configuration options and monitors for changes.</param>
    /// <param name="behaviorSimulator">Simulates reputation evaluation behavior based on configured options.</param>
    public SimulatedEmailReputationService(
        ILogger<SimulatedEmailReputationService> logger,
        string optionsName,
        IOptionsMonitor<ReputationServiceOptions<EmailReputationStubFactoryOptions>> optionsMonitor,
        ReputationBehaviorSimulator<EmailAddressReputation, EmailReputationStubFactoryOptions> behaviorSimulator)
    {
        _logger = logger;
        _optionsName = optionsName;
        _serviceOptions = optionsMonitor.Get(optionsName);
        _optionsObserver = optionsMonitor.OnChange(OnOptionsChanged);
        _behaviorSimulator = behaviorSimulator;
    }

    /// <inheritdoc/>
    public async Task<ErrorOr<EmailAddressReputation>> EvaluateAsync(
        MailAddress data,
        CancellationToken cancellationToken = default)
    {
        return _disposed
            ? throw new ObjectDisposedException($"{nameof(SimulatedEmailReputationService)} already disposed.")
            : await _behaviorSimulator.SimulateGetAsync(
                _serviceOptions,
                cancellationToken);
    }

    /// <inheritdoc/>
    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        _disposed = true;
        _optionsObserver?.Dispose();
    }

    /// <summary>
    /// Handles changes to the analyzer options configuration.
    /// </summary>
    /// <param name="changedOptions">The updated analyzer options configuration.</param>
    /// <param name="name">The name of the options that changed. Used to identify if the change is relevant to this instance.</param>
    private void OnOptionsChanged(
        ReputationServiceOptions<EmailReputationStubFactoryOptions> changedOptions,
        string? name)
    {
        if (name is null || !name.Equals(_optionsName))
        {
            return;
        }

        _serviceOptions = changedOptions;
        _logger.LogDebug("Options with name {Name} were changed", name);
    }
}
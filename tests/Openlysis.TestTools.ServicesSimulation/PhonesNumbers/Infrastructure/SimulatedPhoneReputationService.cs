using ErrorOr;

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

using Openlysis.Application.Common.Abstractions.Contracts;
using Openlysis.Domain.Phones.Entities;
using Openlysis.TestTools.ServicesSimulation.Common.Configuration;
using Openlysis.TestTools.ServicesSimulation.Common.Services.Reputations;
using Openlysis.TestTools.ServicesSimulation.PhonesNumbers.Configuration;

namespace Openlysis.TestTools.ServicesSimulation.PhonesNumbers.Infrastructure;

/// <summary>
/// Simulates a phone numbers reputation service for testing purposes.
/// This class provides mock implementations of phone numbers reputation operations
/// without making actual service calls.
/// </summary>
internal sealed class SimulatedPhoneReputationService
    : IReputationEvaluator<string, PhoneServiceReputation>, IDisposable
{
    /// <inheritdoc/>
    public string ServiceName => _serviceOptions.Name;

    /// <inheritdoc/>
    public bool IsAvailable => _serviceOptions.IsAvailable;

    private readonly ILogger<SimulatedPhoneReputationService> _logger;
    private readonly ReputationBehaviorSimulator<PhoneServiceReputation, PhoneReputationStubFactoryOptions> _behaviorSimulator;
    private readonly string _optionsName;
    private readonly IDisposable? _optionsObserver;
    private ReputationServiceOptions<PhoneReputationStubFactoryOptions> _serviceOptions;
    private bool _disposed;

    /// <summary>
    /// Initializes a new instance of the <see cref="SimulatedPhoneReputationService"/> class.
    /// </summary>
    /// <param name="logger">The logger for recording diagnostic information.</param>
    /// <param name="optionsName">The name used to retrieve the specific options from the options monitor.</param>
    /// <param name="optionsMonitor">The monitor for observing changes to reputation service options.</param>
    /// <param name="behaviorSimulator">The simulator that defines the behavior of reputation evaluations.</param>
    public SimulatedPhoneReputationService(
        ILogger<SimulatedPhoneReputationService> logger,
        string optionsName,
        IOptionsMonitor<ReputationServiceOptions<PhoneReputationStubFactoryOptions>> optionsMonitor,
        ReputationBehaviorSimulator<PhoneServiceReputation, PhoneReputationStubFactoryOptions> behaviorSimulator)
    {
        _logger = logger;
        _optionsName = optionsName;
        _serviceOptions = optionsMonitor.Get(optionsName);
        _optionsObserver = optionsMonitor.OnChange(OnOptionsChanged);
        _behaviorSimulator = behaviorSimulator;
    }

    /// <inheritdoc/>
    public async Task<ErrorOr<PhoneServiceReputation>> EvaluateAsync(
        string data,
        CancellationToken cancellationToken = default)
    {
        return _disposed
            ? throw new ObjectDisposedException($"{nameof(SimulatedPhoneReputationService)} already disposed.")
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
        ReputationServiceOptions<PhoneReputationStubFactoryOptions> changedOptions,
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
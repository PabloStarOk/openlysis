using ErrorOr;

using Microsoft.Extensions.Logging;

using Openlysis.Domain.Common.Entities;
using Openlysis.TestTools.ServicesSimulation.Common.Configuration;
using Openlysis.TestTools.ServicesSimulation.Common.Constants;
using Openlysis.TestTools.ServicesSimulation.Common.Services.Abstractions;

namespace Openlysis.TestTools.ServicesSimulation.Common.Services.Reputations;

/// <summary>
/// Simulates reputation-related behavior for testing services.
/// </summary>
/// <typeparam name="TReputation">The type of service reputation to simulate, must derive from ServiceReputation.</typeparam>
/// <typeparam name="TStubFactoryOptions">The options for configuring stub factories, must derive from StubFactoryOptions.</typeparam>
internal sealed class ReputationBehaviorSimulator<TReputation, TStubFactoryOptions>
    where TReputation : ServiceReputation
    where TStubFactoryOptions : StubFactoryOptions
{
    private readonly ILogger<ReputationBehaviorSimulator<TReputation, TStubFactoryOptions>> _logger;
    private readonly StubFactory<TStubFactoryOptions, TReputation> _reputationFactory;

    /// <summary>
    /// Initializes a new instance of the <see cref="ReputationBehaviorSimulator{TReputation, TStubFactoryOptions}"/> class.
    /// </summary>
    /// <param name="logger">The logger used for diagnostic logging within the simulator.</param>
    /// <param name="reputationFactory">The factory responsible for creating reputation stubs.</param>
    public ReputationBehaviorSimulator(
        ILogger<ReputationBehaviorSimulator<TReputation, TStubFactoryOptions>> logger,
        StubFactory<TStubFactoryOptions, TReputation> reputationFactory)
    {
        _logger = logger;
        _reputationFactory = reputationFactory;
    }

    /// <summary>
    /// Simulates retrieving a reputation asynchronously with configurable behavior.
    /// </summary>
    /// <param name="serviceOptions">Configuration options for the reputation service.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>
    /// An <see cref="ErrorOr{TValue}"/> containing either a <typeparamref name="TReputation"/>
    /// instance or an error if the simulation is configured to return errors.
    /// </returns>
    internal async Task<ErrorOr<TReputation>> SimulateGetAsync(
        ReputationServiceOptions<TStubFactoryOptions> serviceOptions,
        CancellationToken cancellationToken = default)
    {
        LogRequestReceived(serviceOptions, nameof(SimulateGetAsync));

        SimulatedEndpointOptions endpointOptions =
            serviceOptions.GetReputationEndpoint;
        if (endpointOptions.ReturnError)
        {
            return SimulationErrors.GetReputation;
        }

        var latency = TimeSpan.FromMilliseconds(endpointOptions.LatencyMs);
        await Task.Delay(latency, cancellationToken);

        return _reputationFactory.Create(
            serviceOptions.Name,
            serviceOptions.StubFactory);
    }

    /// <summary>
    /// Logs information about a received request to the service.
    /// </summary>
    /// <param name="serviceOptions">The configuration options for the reputation service.</param>
    /// <param name="methodName">The name of the method being called.</param>
    private void LogRequestReceived(
        ReputationServiceOptions<TStubFactoryOptions> serviceOptions,
        string methodName)
    {
        _logger.LogDebug(
            "{ServiceName}: Behavior simulated with {MethodName} method.",
            serviceOptions.Name,
            methodName);
    }
}
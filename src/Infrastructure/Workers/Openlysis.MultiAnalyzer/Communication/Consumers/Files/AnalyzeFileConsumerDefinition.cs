using MassTransit;

using Microsoft.Extensions.Options;

using Openlysis.Infrastructure.Shared.Communication.Configuration;

namespace Openlysis.MultiAnalyzer.Communication.Consumers.Files;

/// <summary>
/// Defines the consumer for file multi-analysis.
/// </summary>
internal sealed class AnalyzeFileConsumerDefinition
    : ConsumerDefinition<AnalyzeFileConsumer>
{
    private readonly IOptions<ConsumersOptions> _consumersOptions;

    /// <summary>
    /// Initializes a new instance of the <see cref="AnalyzeFileConsumerDefinition"/> class.
    /// </summary>
    /// <param name="consumersOptions">The options for configuring consumers.</param>
    public AnalyzeFileConsumerDefinition(IOptions<ConsumersOptions> consumersOptions)
    {
        EndpointName = consumersOptions.Value.AnalyzeFile.Name;
        _consumersOptions = consumersOptions;
    }

    /// <inheritdoc/>
    protected override void ConfigureConsumer(
        IReceiveEndpointConfigurator endpointConfigurator,
        IConsumerConfigurator<AnalyzeFileConsumer> consumerConfigurator,
        IRegistrationContext context)
    {
        ConsumersOptions options = _consumersOptions.Value;

        endpointConfigurator.ConcurrentMessageLimit = options.AnalyzeFile.ConcurrencyLimit;
        endpointConfigurator.PrefetchCount = options.AnalyzeFile.ConcurrencyLimit;

        endpointConfigurator.UseMessageRetry(r => r.Intervals(options.AnalyzeFile.RetryIntervals));
    }
}
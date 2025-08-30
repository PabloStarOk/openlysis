using MassTransit;

using Microsoft.Extensions.Options;

using Openlysis.Infrastructure.Shared.Communication.Configuration;

namespace Openlysis.MultiAnalyzer.Communication.Consumers.URLs;

/// <summary>
/// Defines the consumer for analyzing URLs.
/// </summary>
internal sealed class AnalyzeUrlConsumerDefinition
    : ConsumerDefinition<AnalyzeUrlConsumer>
{
    private readonly IOptions<ConsumersOptions> _consumersOptions;

    /// <summary>
    /// Initializes a new instance of the <see cref="AnalyzeUrlConsumerDefinition"/> class.
    /// </summary>
    /// <param name="consumersOptions">The options for configuring consumers.</param>
    public AnalyzeUrlConsumerDefinition(IOptions<ConsumersOptions> consumersOptions)
    {
        EndpointName = consumersOptions.Value.AnalyzeUrl.Name;
        _consumersOptions = consumersOptions;
    }

    /// <inheritdoc/>
    protected override void ConfigureConsumer(
        IReceiveEndpointConfigurator endpointConfigurator,
        IConsumerConfigurator<AnalyzeUrlConsumer> consumerConfigurator,
        IRegistrationContext context)
    {
        ConsumersOptions options = _consumersOptions.Value;

        endpointConfigurator.ConcurrentMessageLimit = options.AnalyzeUrl.ConcurrencyLimit;
        endpointConfigurator.PrefetchCount = options.AnalyzeUrl.ConcurrencyLimit;

        endpointConfigurator.UseMessageRetry(r => r.Intervals(options.AnalyzeUrl.RetryIntervals));
    }
}
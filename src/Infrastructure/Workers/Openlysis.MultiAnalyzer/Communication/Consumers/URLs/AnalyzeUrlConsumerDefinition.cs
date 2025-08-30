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
    /// <summary>
    /// Initializes a new instance of the <see cref="AnalyzeUrlConsumerDefinition"/> class.
    /// </summary>
    /// <param name="consumersOptions">The options for configuring consumers.</param>
    public AnalyzeUrlConsumerDefinition(IOptions<ConsumersOptions> consumersOptions)
    {
        EndpointName = consumersOptions.Value.AnalyzeUrl.Name;
    }
}
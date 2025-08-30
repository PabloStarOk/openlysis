using MassTransit;

using Microsoft.Extensions.Options;

using Openlysis.Domain.URLs;
using Openlysis.Domain.URLs.Entities;
using Openlysis.Infrastructure.Communication.Consumers.Common;
using Openlysis.Infrastructure.Shared.Communication.Configuration;

namespace Openlysis.Infrastructure.Communication.Consumers.URLs;

/// <summary>
/// Defines the consumer for updating URL multi-analysis.
/// </summary>
internal sealed class UpdateUrlMultiAnalysisConsumerDefinition
    : ConsumerDefinition<UpdateMultiAnalysisConsumer<UrlMultiAnalysis, UrlAnalysis>>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="UpdateUrlMultiAnalysisConsumerDefinition"/> class.
    /// </summary>
    /// <param name="consumersOptions">The options for configuring consumers.</param>
    public UpdateUrlMultiAnalysisConsumerDefinition(IOptions<ConsumersOptions> consumersOptions)
    {
        EndpointName = consumersOptions.Value.UpdateUrlAnalysis.Name;
    }
}
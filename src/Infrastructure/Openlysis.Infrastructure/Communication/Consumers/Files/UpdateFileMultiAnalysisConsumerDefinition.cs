using MassTransit;

using Microsoft.Extensions.Options;

using Openlysis.Domain.Files;
using Openlysis.Domain.Files.Entities;
using Openlysis.Infrastructure.Communication.Consumers.Common;
using Openlysis.Infrastructure.Shared.Communication.Configuration;

namespace Openlysis.Infrastructure.Communication.Consumers.Files;

/// <summary>
/// Defines the consumer for updating file multi-analysis.
/// </summary>
internal sealed class UpdateFileMultiAnalysisConsumerDefinition
    : ConsumerDefinition<UpdateMultiAnalysisConsumer<FileMultiAnalysis, FileAnalysis>>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="UpdateFileMultiAnalysisConsumerDefinition"/> class.
    /// </summary>
    /// <param name="brokerOptions">The broker settings options used to configure the endpoint name.</param>
    public UpdateFileMultiAnalysisConsumerDefinition(
        IOptions<BrokerSettings> brokerOptions)
    {
        EndpointName = brokerOptions.Value.UpdateFileAnalysisEndpointName;
    }
}
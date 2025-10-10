using System;

using MassTransit;

using Microsoft.Extensions.Options;

using Openlysis.AnalysisOrchestrator.Communication.Consumers.Common;
using Openlysis.AnalysisOrchestrator.Configuration;
using Openlysis.Domain.URLs.Entities;
using Openlysis.Infrastructure.Shared.Communication.Configuration;
using Openlysis.Infrastructure.Shared.Communication.Contracts;

namespace Openlysis.AnalysisOrchestrator.Communication.Consumers.URLs;

/// <summary>
/// Defines the consumer for URL analysis jobs, specifying configuration and options
/// for processing <see cref="UrlAnalysisJobMessage"/> messages using <see cref="AnalysisJobConsumer{TAnalysis,TMessage}"/>.
/// </summary>
internal sealed class UrlAnalysisJobConsumerDefinition
    : ConsumerDefinition<AnalysisJobConsumer<UrlAnalysis, UrlAnalysisJobMessage>>
{
    private readonly IOptions<ConsumersOptions> _consumersOptions;
    private readonly IOptions<JobTimeoutOptions> _timeoutOptions;

    /// <summary>
    /// Initializes a new instance of the <see cref="UrlAnalysisJobConsumerDefinition"/> class.
    /// </summary>
    /// <param name="consumersOptions">Options for configuring consumers, including endpoint and concurrency settings.</param>
    /// <param name="timeoutOptions">Options for configuring job timeout and cancellation settings.</param>
    public UrlAnalysisJobConsumerDefinition(
        IOptions<ConsumersOptions> consumersOptions,
        IOptions<JobTimeoutOptions> timeoutOptions)
    {
        EndpointName = consumersOptions.Value.AnalyzeUrl.Name;
        _consumersOptions = consumersOptions;
        _timeoutOptions = timeoutOptions;
    }

    /// <inheritdoc/>
    protected override void ConfigureConsumer(
        IReceiveEndpointConfigurator endpointConfigurator,
        IConsumerConfigurator<AnalysisJobConsumer<UrlAnalysis, UrlAnalysisJobMessage>> consumerConfigurator,
        IRegistrationContext context)
    {
        ConsumersOptions options = _consumersOptions.Value;

        endpointConfigurator.PrefetchCount = options.AnalyzeUrl.ConcurrencyLimit;

        consumerConfigurator.Options<JobOptions<UrlAnalysisJobMessage>>(jobOptions =>
        {
            var timeout = TimeSpan.FromSeconds(_timeoutOptions.Value.TimeoutSeconds);
            var timeoutCancellation = TimeSpan.FromSeconds(_timeoutOptions.Value.TimeoutCancellationSeconds);

            jobOptions
                .SetJobTimeout(timeout)
                .SetJobCancellationTimeout(timeoutCancellation)
                .SetRetry(retryConfigurator =>
                {
                    retryConfigurator.Intervals(options.AnalyzeUrl.RetryIntervals);
                })
                .SetConcurrentJobLimit(options.AnalyzeUrl.ConcurrencyLimit)
                .SetGlobalConcurrentJobLimit(options.AnalyzeUrl.ConcurrencyLimit);
        });
    }
}
using System;

using MassTransit;

using Microsoft.Extensions.Options;

using Openlysis.Domain.Files.Entities;
using Openlysis.Infrastructure.Shared.Communication.Configuration;
using Openlysis.Infrastructure.Shared.Communication.Contracts;
using Openlysis.MultiAnalyzer.Communication.Consumers.Common;
using Openlysis.MultiAnalyzer.Configuration;

namespace Openlysis.MultiAnalyzer.Communication.Consumers.Files;

/// <summary>
/// Defines the consumer for file analysis jobs, specifying configuration and options
/// for processing <see cref="FileAnalysisJobMessage"/> messages using <see cref="AnalysisJobConsumer{TAnalysis, TMessage}"/>.
/// </summary>
internal sealed class FileAnalysisJobConsumerDefinition
    : ConsumerDefinition<AnalysisJobConsumer<FileAnalysis, FileAnalysisJobMessage>>
{
    private readonly IOptions<ConsumersOptions> _consumersOptions;
    private readonly IOptions<JobTimeoutOptions> _timeoutOptions;

    /// <summary>
    /// Initializes a new instance of the <see cref="FileAnalysisJobConsumerDefinition"/> class.
    /// </summary>
    /// <param name="consumersOptions">Options for configuring consumers, including endpoint and concurrency settings.</param>
    /// <param name="timeoutOptions">Options for configuring job timeout and cancellation settings.</param>
    public FileAnalysisJobConsumerDefinition(
        IOptions<ConsumersOptions> consumersOptions,
        IOptions<JobTimeoutOptions> timeoutOptions)
    {
        EndpointName = consumersOptions.Value.AnalyzeFile.Name;
        _consumersOptions = consumersOptions;
        _timeoutOptions = timeoutOptions;
    }

    /// <inheritdoc/>
    protected override void ConfigureConsumer(
        IReceiveEndpointConfigurator endpointConfigurator,
        IConsumerConfigurator<AnalysisJobConsumer<FileAnalysis, FileAnalysisJobMessage>> consumerConfigurator,
        IRegistrationContext context)
    {
        ConsumersOptions options = _consumersOptions.Value;

        endpointConfigurator.PrefetchCount = options.AnalyzeFile.ConcurrencyLimit;

        consumerConfigurator.Options<JobOptions<FileAnalysisJobMessage>>(jobOptions =>
        {
            var timeout = TimeSpan.FromSeconds(_timeoutOptions.Value.TimeoutSeconds);
            var timeoutCancellation = TimeSpan.FromSeconds(_timeoutOptions.Value.TimeoutCancellationSeconds);

            jobOptions
                .SetJobTimeout(timeout)
                .SetJobCancellationTimeout(timeoutCancellation)
                .SetRetry(retryConfigurator =>
                {
                    retryConfigurator.Intervals(options.AnalyzeFile.RetryIntervals);
                })
                .SetConcurrentJobLimit(options.AnalyzeFile.ConcurrencyLimit)
                .SetGlobalConcurrentJobLimit(options.AnalyzeFile.ConcurrencyLimit);
        });
    }
}
using System;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

using Openlysis.Domain.Common.Entities;
using Openlysis.Infrastructure.Shared.Communication.Contracts;
using Openlysis.MultiAnalyzer.Abstractions;

namespace Openlysis.MultiAnalyzer.Infrastructure.TimeoutRequests;

/// <summary>
/// Factory for creating orchestrators for timeout analysis requests.
/// </summary>
/// <typeparam name="TAnalysis">The type of analysis entity.</typeparam>
/// <typeparam name="TRequest">The type of request handled.</typeparam>
internal sealed class TimeoutAnalysisRequestCoreFactory<TAnalysis, TRequest>
    : TimeoutRequestFactory<TRequest>
    where TAnalysis : Analysis
    where TRequest : QueueMessage
{
    /// <summary>
    /// Initializes a new instance of the <see cref="TimeoutAnalysisRequestCoreFactory{TAnalysis, TRequest}"/> class.
    /// </summary>
    /// <param name="loggerFactory">The logger factory to use for logging.</param>
    /// <param name="serviceProvider">The service provider for dependency resolution.</param>
    /// <param name="scopeFactory">The factory for creating service scopes.</param>
    public TimeoutAnalysisRequestCoreFactory(
        ILoggerFactory loggerFactory,
        IServiceProvider serviceProvider,
        IServiceScopeFactory scopeFactory)
        : base(loggerFactory, serviceProvider, scopeFactory)
    {
    }

    /// <inheritdoc/>
    protected override IRequestOrchestrator<TRequest> CreateCore(
        Guid timeoutRequestId)
    {
        IServiceScope scope = CreateScope(timeoutRequestId);
        return scope.ServiceProvider
            .GetRequiredService<MultiAnalysisOrchestrator<TAnalysis, TRequest>>();
    }
}
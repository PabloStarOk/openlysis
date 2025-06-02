using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

using Openlysis.MultiAnalyzer.Configuration;
using Openlysis.MultiAnalyzer.Models;

namespace Openlysis.MultiAnalyzer.Abstractions;

/// <summary>
/// Defines a factory for creating and disposing <see cref="TimeoutRequest{TRequest}"/> instances.
/// </summary>
/// <typeparam name="TRequest">The type of request to be processed. Must be non-nullable.</typeparam>
internal abstract class TimeoutRequestFactory<TRequest>
    : IDisposable, IAsyncDisposable
    where TRequest : class
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILoggerFactory _loggerFactory;
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ConcurrentDictionary<Guid, AsyncServiceScope> _createdScopes = [];
    private bool _disposed;

    /// <summary>
    /// Initializes a new instance of the <see cref="TimeoutRequestFactory{TRequest}"/> class.
    /// </summary>
    /// <param name="loggerFactory">The factory used to create logger instances for logging within the factory and timeout requests.</param>
    /// <param name="serviceProvider">The root <see cref="IServiceProvider"/> for resolving dependencies.</param>
    /// <param name="scopeFactory">The factory for creating service scopes, enabling scoped dependency resolution for each timeout request.</param>
    protected TimeoutRequestFactory(
        ILoggerFactory loggerFactory,
        IServiceProvider serviceProvider,
        IServiceScopeFactory scopeFactory)
    {
        _serviceProvider = serviceProvider;
        _loggerFactory = loggerFactory;
        _scopeFactory = scopeFactory;
    }

    /// <inheritdoc/>
    public void Dispose()
    {
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }

    /// <inheritdoc/>
    public async ValueTask DisposeAsync()
    {
        await DisposeAsync(disposing: true).ConfigureAwait(false);
        GC.SuppressFinalize(this);
    }

    /// <summary>
    /// Creates a new instance of <see cref="TimeoutRequest{TRequest}"/>.
    /// </summary>
    /// <returns>A new <see cref="TimeoutRequest{TRequest}"/> instance.</returns>
    internal TimeoutRequest<TRequest> Create()
    {
        var logger = _loggerFactory.CreateLogger<TimeoutRequest<TRequest>>();

        var options = _serviceProvider
            .GetRequiredService<IOptions<TimeoutRequestOptions>>();

        var timeProvider = _serviceProvider
            .GetRequiredService<TimeProvider>();

        var id = Guid.NewGuid();

        IRequestOrchestrator<TRequest> orchestrator = CreateCore(id);
        return new TimeoutRequest<TRequest>(
            logger,
            id,
            options.Value,
            timeProvider,
            orchestrator);
    }

    /// <summary>
    /// Asynchronously disposes the specified <see cref="TimeoutRequest{TRequest}"/> instance and its associated scope.
    /// </summary>
    /// <param name="timeoutRequest">The timeout request instance to dispose.</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous dispose operation.</returns>
    internal async Task DisposeRequestAsync(TimeoutRequest<TRequest> timeoutRequest)
    {
        await timeoutRequest.DisposeAsync().ConfigureAwait(false);
        if (!_createdScopes.TryRemove(timeoutRequest.Id, out AsyncServiceScope scope))
        {
            return;
        }

        await scope.DisposeAsync().ConfigureAwait(false);
    }

    /// <summary>
    /// Creates a new <see cref="IServiceScope"/> for the specified timeout request ID and tracks it for disposal.
    /// </summary>
    /// <param name="timeoutRequestId">The unique identifier for the timeout request instance.</param>
    /// <returns>A new <see cref="IServiceScope"/> associated with the timeout request.</returns>
    protected IServiceScope CreateScope(Guid timeoutRequestId)
    {
        AsyncServiceScope scope = _scopeFactory.CreateAsyncScope();
        _createdScopes.TryAdd(timeoutRequestId, scope);
        return scope;
    }

    /// <summary>
    /// Releases all resources used by the factory, including any tracked service scopes for timeout requests.
    /// </summary>
    /// <param name="disposing">
    /// true to release both managed and unmanaged resources; false to release only unmanaged resources.
    /// </param>
    protected virtual void Dispose(bool disposing)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);

        if (!disposing)
        {
            return;
        }

        _disposed = true;
        foreach (AsyncServiceScope scope in _createdScopes.Values)
        {
            scope.Dispose();
        }

        _createdScopes.Clear();
    }

    /// <summary>
    /// Asynchronously releases all resources used by the factory, including any tracked service scopes for timeout requests.
    /// </summary>
    /// <param name="disposing">
    /// Indicates whether the method is called from a direct call to DisposeAsync (true)
    /// or from a finalizer (false).
    /// </param>
    /// <returns>A <see cref="Task"/> representing the asynchronous dispose operation.</returns>
    protected virtual async Task DisposeAsync(bool disposing)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);

        if (!disposing)
        {
            return;
        }

        _disposed = true;
        foreach (AsyncServiceScope scope in _createdScopes.Values)
        {
            await scope.DisposeAsync().ConfigureAwait(false);
        }

        _createdScopes.Clear();
    }

    /// <summary>
    /// Creates an instance of <see cref="IRequestOrchestrator{TRequest}"/> for the specified timeout request ID.
    /// </summary>
    /// <param name="timeoutRequestId">The unique identifier for the timeout request instance.</param>
    /// <returns>An instance of <see cref="IRequestOrchestrator{TRequest}"/>.</returns>
    protected abstract IRequestOrchestrator<TRequest> CreateCore(Guid timeoutRequestId);
}

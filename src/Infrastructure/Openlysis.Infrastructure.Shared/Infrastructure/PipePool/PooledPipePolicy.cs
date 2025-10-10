using System.IO.Pipelines;

using Microsoft.Extensions.ObjectPool;

namespace Openlysis.Infrastructure.Shared.Infrastructure.PipePool;

/// <summary>
/// An <see cref="IPooledObjectPolicy{Pipe}"/> implementation for pooling <see cref="Pipe"/> instances
/// with specified <see cref="PipeOptions"/>.
/// </summary>
internal sealed class PooledPipePolicy : IPooledObjectPolicy<Pipe>
{
    private readonly PipeOptions _pipeOptions;

    /// <summary>
    /// Initializes a new instance of the <see cref="PooledPipePolicy"/> class with the specified <see cref="PipeOptions"/>.
    /// </summary>
    /// <param name="pipeOptions">The options to configure the pooled <see cref="Pipe"/> instances.</param>
    public PooledPipePolicy(PipeOptions pipeOptions)
    {
        _pipeOptions = pipeOptions;
    }

    /// <inheritdoc/>
    public Pipe Create()
    {
        return new Pipe(_pipeOptions);
    }

    /// <inheritdoc/>
    public bool Return(Pipe obj)
    {
        obj.Reset();
        return true;
    }
}
using System.Buffers;
using System.IO.Pipelines;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.ObjectPool;

namespace Openlysis.Infrastructure.Shared.Infrastructure.PipePool;

/// <summary>
/// Provides extension methods for registering pipe pooling services in the dependency injection container.
/// </summary>
public static class DependencyInjection
{
    /// <summary>
    /// Registers services required for pooling <see cref="Pipe"/> instances.
    /// </summary>
    /// <param name="services">The service collection to add the pool to.</param>
    /// <param name="minimumSegmentSize">The minimum size of memory segments for pipes. Default is 65,536 bytes.</param>
    public static void AddPipePool(
        this IServiceCollection services,
        int minimumSegmentSize = 65536)
    {
        services.AddSingleton(_ => MemoryPool<byte>.Shared);
        services.AddSingleton<PipeOptions>(sp =>
            {
                var memoryPool = sp.GetRequiredService<MemoryPool<byte>>();
                return new PipeOptions(memoryPool, minimumSegmentSize: minimumSegmentSize);
            });
        services.AddSingleton<IPooledObjectPolicy<Pipe>, PooledPipePolicy>();
        services.AddSingleton<ObjectPoolProvider, DefaultObjectPoolProvider>();
        services.AddSingleton<ObjectPool<Pipe>>(sp =>
        {
            var poolProvider = sp.GetRequiredService<ObjectPoolProvider>();
            var poolPolicy = sp.GetRequiredService<IPooledObjectPolicy<Pipe>>();
            return poolProvider.Create(poolPolicy);
        });
    }
}
using Openlysis.Domain.Common.Entities;

namespace Openlysis.Application.Common.Abstractions.Services;

/// <summary>
/// Defines a service to hash data.
/// </summary>
public interface IHashService
{
    /// <summary>
    /// Hashes the given data into a <see cref="HashValues"/>.
    /// </summary>
    /// <param name="data">A <see cref="Stream"/> with the data.</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/> to stop the operation.</param>
    /// <returns>A <see cref="HashValues"/>.</returns>
    public Task<HashValues> HashDataAsync(Stream data, CancellationToken cancellationToken = default);

    /// <summary>
    /// Hashes the given byte data and returns a <see cref="HashValues"/>.
    /// </summary>
    /// <param name="data">A read-only span of bytes containing the data to hash.</param>
    /// <returns>A <see cref="HashValues"/> representing the hash of the data.</returns>
    public HashValues HashData(ReadOnlySpan<byte> data);
}
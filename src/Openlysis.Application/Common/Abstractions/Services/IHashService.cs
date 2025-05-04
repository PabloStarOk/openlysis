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
    public Task<HashValues> HashDataAsync(Stream data, CancellationToken cancellationToken);
}
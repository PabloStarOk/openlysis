using Openlysis.Domain.Common.Entities;

namespace Openlysis.Application.Common.Abstractions.Services;

/// <summary>
/// Defines a service to hash data.
/// </summary>
public interface IHashService
{
    /// <summary>
    /// Hashes the given data into a <see cref="ContentHashSet"/>.
    /// </summary>
    /// <param name="data">A <see cref="Stream"/> with the data.</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/> to stop the operation.</param>
    /// <returns>A <see cref="ContentHashSet"/>.</returns>
    public Task<ContentHashSet> HashDataAsync(Stream data, CancellationToken cancellationToken);
}
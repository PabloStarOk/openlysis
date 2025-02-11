using Openlysis.Domain.Common.Hash;

namespace Openlysis.Application.Common.Interfaces.Services;

/// <summary>
/// Defines a service to hash data.
/// </summary>
public interface IHashService
{
    /// <summary>
    /// Hashes the given data into a <see cref="ContentHashSet"/>.
    /// </summary>
    /// <param name="data">A <see cref="Stream"/> with the data.</param>
    /// <returns>A <see cref="ContentHashSet"/>.</returns>
    public Task<ContentHashSet> HashDataAsync(Stream data);
}
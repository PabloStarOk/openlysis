using Openlysis.Domain.Common.Hash;

namespace Openlysis.Application.Common.Interfaces.Services;

/// <summary>
/// Defines a service to hash data.
/// </summary>
public interface IHashService
{
    /// <summary>
    /// Hashes the given data into a <see cref="HashSet"/>.
    /// </summary>
    /// <param name="data">A <see cref="Stream"/> with the data.</param>
    /// <returns>A <see cref="HashSet"/>.</returns>
    public Task<HashSet> HashDataAsync(Stream data);
}
using Openlysis.Assessors.Shared.Models.Common;

namespace Openlysis.Assessors.Shared.Abstractions;

/// <summary>
/// Defines a factory interface for creating endpoint addresses to assess data.
/// </summary>
/// <typeparam name="TData">The type of data to be assessed.</typeparam>
public interface IEndpointAddressFactory<in TData>
    where TData : AssessedData
{
    /// <summary>
    /// Creates a URI for the given data content.
    /// </summary>
    /// <param name="content">The data content to create the URI for.</param>
    /// <returns>A URI for the given data content.</returns>
    public Uri Create(TData content);
}
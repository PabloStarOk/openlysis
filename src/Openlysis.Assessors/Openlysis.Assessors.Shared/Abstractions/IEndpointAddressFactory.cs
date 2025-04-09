using Openlysis.Application.Common.Requests;

namespace Openlysis.Assessors.Shared.Abstractions;

/// <summary>
/// Defines a factory interface for creating endpoint addresses to assess data.
/// </summary>
/// <typeparam name="TData">The type of data to be assessed.</typeparam>
public interface IEndpointAddressFactory<in TData>
    where TData : AssessData
{
    /// <summary>
    /// Creates a URI for the given data.
    /// </summary>
    /// <param name="data">The data to create the URI for.</param>
    /// <returns>A URI for the given data.</returns>
    public Uri Create(TData data);
}
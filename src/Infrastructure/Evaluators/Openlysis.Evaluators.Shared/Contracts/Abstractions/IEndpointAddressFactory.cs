namespace Openlysis.Evaluators.Shared.Contracts.Abstractions;

/// <summary>
/// Defines a factory interface for creating endpoint addresses (URIs)
/// to evaluate the reputation of <see cref="TData"/>.
/// </summary>
/// <typeparam name="TData">The type of the data to be evaluated.</typeparam>
public interface IEndpointAddressFactory<in TData>
    where TData : notnull
{
    /// <summary>
    /// Creates a URI for the given request data.
    /// </summary>
    /// <param name="data">The request data used to create the URI.</param>
    /// <returns>A URI representing the endpoint address for the given data.</returns>
    public Uri Create(TData data);
}
using Openlysis.Application.Phones.Contracts.Requests;

namespace Openlysis.Evaluators.Shared.Contracts.Abstractions;

/// <summary>
/// Defines a factory interface for creating endpoint addresses (URIs)
/// to evaluate the reputation according to an implementation of
/// <see cref="EvaluateReputationRequest"/>.
/// </summary>
/// <typeparam name="TRequest">
/// The type of request for which the endpoint address will be created.
/// Must inherit from <see cref="EvaluateReputationRequest"/>.
/// </typeparam>
public interface IEndpointAddressFactory<in TRequest>
    where TRequest : EvaluateReputationRequest
{
    /// <summary>
    /// Creates a URI for the given request data.
    /// </summary>
    /// <param name="data">The request data used to create the URI.</param>
    /// <returns>A URI representing the endpoint address for the given data.</returns>
    public Uri Create(TRequest data);
}
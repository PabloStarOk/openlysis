namespace Openlysis.Application.Phones.Contracts.Requests;

/// <summary>
/// Represents a request to evaluate the reputation of data.
/// </summary>
/// <param name="Value">The value to be evaluated for reputation.</param>
public abstract record EvaluateReputationRequest(string Value)
{
    /// <summary>
    /// Validates the format of the data entity associated with the request.
    /// </summary>
    /// <returns>
    /// True if the data entity format is valid; otherwise, false.
    /// </returns>
    public abstract bool IsFormatValid();
}
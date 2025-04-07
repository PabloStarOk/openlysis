using ErrorOr;

namespace Openlysis.Infrastructure.Shared.Constants;

/// <summary>
/// Contains error constants used throughout the infrastructure services.
/// </summary>
public static class ServiceErrors
{
    /// <summary>
    /// Error indicating that the response status code was not successful.
    /// </summary>
    public static readonly Error NonSuccessStatusCode = Error.Unexpected("Response.NotSuccessful", "Response status code was not successful.");

    /// <summary>
    /// Error indicating a failure occurred while trying to deserialize a response.
    /// </summary>
    public static readonly Error DeserializationFailure = Error.Unexpected("Response.DeserializationFailure", "A failure occurred while trying to deserialize a response."); 

    /// <summary>
    /// Error indicating that an object was null after deserialization.
    /// </summary>
    public static readonly Error DeserializationNull = Error.Unexpected("Response.DeserializationNull", "An object was null after deserialization.");
}
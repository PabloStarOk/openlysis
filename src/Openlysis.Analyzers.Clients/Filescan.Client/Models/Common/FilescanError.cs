namespace Filescan.Client.Models.Common;

/// <summary>
/// Represents an error response with a detail message.
/// </summary>
/// <param name="IsValidationError">Indicates if the error comes from a validation.</param>
/// <param name="Detail">The detail message of the error.</param>
/// <param name="ValidationErrors">An array of validation errors associated with this error.</param>
public record FilescanError(
    bool IsValidationError,
    string Detail,
    ValidationError[] ValidationErrors)
{
    /// <summary>
    /// Gets an empty error.
    /// </summary>
    public static FilescanError Empty => new (false, string.Empty, []);
}
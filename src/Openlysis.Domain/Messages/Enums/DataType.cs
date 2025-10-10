namespace Openlysis.Domain.Messages.Enums;

/// <summary>
/// Represents the type of data being handled.
/// </summary>
public enum DataType
{
    /// <summary>
    /// Represents a file data type.
    /// </summary>
    File,

    /// <summary>
    /// Represents a URL data type.
    /// </summary>
    Url,

    /// <summary>
    /// Represents an email address data type.
    /// </summary>
    EmailAddress,

    /// <summary>
    /// Represents a phone number data type.
    /// </summary>
    PhoneNumber,
}
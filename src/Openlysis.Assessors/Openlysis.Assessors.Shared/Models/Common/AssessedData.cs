namespace Openlysis.Assessors.Shared.Models.Common;

/// <summary>
/// Represents a data entity such as a phone number, email, etc.
/// </summary>
/// <param name="Value">The value of the data entity.</param>
public abstract record AssessedData(string Value)
{
    /// <summary>
    /// Validates the format of data entity.
    /// </summary>
    /// <returns>
    /// True if the data entity format is valid; otherwise, false.
    /// </returns>
    public abstract bool IsFormatValid();
}
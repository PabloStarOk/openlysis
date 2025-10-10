using System.Diagnostics.CodeAnalysis;

using Medo;

namespace Openlysis.Domain.Common.ValueObjects;

/// <summary>
/// Represents a generic unique identifier.
/// </summary>
public record GlobalId
{
    /// <summary>
    /// Gets the value of the <see cref="GlobalId"/>.
    /// </summary>
    public Guid Value { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="GlobalId"/> class with the specified value.
    /// </summary>
    /// <param name="value">The unique identifier for the analysis.</param>
    private GlobalId(Guid value)
    {
        Value = value;
    }

    /// <summary>
    /// Creates a new instance of <see cref="GlobalId"/> with a unique identifier.
    /// </summary>
    /// <returns>A new <see cref="GlobalId"/> instance with a unique identifier.</returns>
    public static GlobalId CreateUnique()
    {
        return new GlobalId(Uuid7.NewUuid7());
    }

    /// <summary>
    /// Parses the specified string into a <see cref="GlobalId"/>.
    /// </summary>
    /// <param name="input">The string representation of the unique identifier.</param>
    /// <returns>A <see cref="GlobalId"/> instance parsed from the input string.</returns>
    /// <exception cref="FormatException">Thrown if the input is not a valid ID string.</exception>
    public static GlobalId Parse(string input)
    {
        return new GlobalId(Uuid7.Parse(input));
    }

    /// <summary>
    /// Attempts to parse the specified string into a <see cref="GlobalId"/>.
    /// </summary>
    /// <param name="input">The string representation of the unique identifier.</param>
    /// <param name="result">When this method returns, contains the <see cref="GlobalId"/> value equivalent to the input string, if the conversion succeeded, or <c>null</c> if the conversion failed.</param>
    /// <returns><c>true</c> if the input was converted successfully; otherwise, <c>false</c>.</returns>
    public static bool TryParse(string input, [NotNullWhen(true)] out GlobalId? result)
    {
        result = null;

        if (!Guid.TryParse(input, out Guid guid))
        {
            return false;
        }

        result = new GlobalId(guid);
        return true;
    }

    /// <inheritdoc/>
    public override string ToString()
    {
        return Value.ToString();
    }
}
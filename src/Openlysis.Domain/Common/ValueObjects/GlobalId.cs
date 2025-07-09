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
    /// Parses the specified <see cref="Guid"/> and returns a new instance of <see cref="GlobalId"/>.
    /// </summary>
    /// <param name="guid">The GUID to parse into a <see cref="GlobalId"/>.</param>
    /// <returns>A new <see cref="GlobalId"/> instance created from the specified GUID.</returns>
    public static GlobalId Parse(Guid guid)
    {
        return new GlobalId(guid);
    }
}
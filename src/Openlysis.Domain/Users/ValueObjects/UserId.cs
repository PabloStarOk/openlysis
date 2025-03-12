namespace Openlysis.Domain.Users.ValueObjects;

/// <summary>
/// Represents a unique identifier for a user.
/// </summary>
public record UserId
{
    /// <summary>
    /// Gets the value of the user identifier.
    /// </summary>
    public Guid Value { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="UserId"/> class.
    /// </summary>
    /// <param name="value">The GUID value of the user identifier.</param>
    private UserId(Guid value)
    {
        Value = value;
    }

    /// <summary>
    /// Creates a new instance of the <see cref="UserId"/> class.
    /// </summary>
    /// <param name="value">The GUID value of the user identifier.</param>
    /// <returns>A new instance of the <see cref="UserId"/> class.</returns>
    public static UserId Create(Guid value)
    {
        return new UserId(value);
    }
}
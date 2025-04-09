using Openlysis.Domain.Common.MultiAnalyses.ValueObjects;

namespace Openlysis.Domain.Common.ValueObjects;

/// <summary>
/// Represents a generic unique identifier.
/// </summary>
public record Id
{
    /// <summary>
    /// Gets the value of the <see cref="Id"/>.
    /// </summary>
    public Guid Value { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="Id"/> class with the specified value.
    /// </summary>
    /// <param name="value">The unique identifier for the analysis.</param>
    private Id(Guid value)
    {
        Value = value;
    }

    /// <summary>
    /// Creates a new instance of <see cref="Id"/> with a unique identifier.
    /// </summary>
    /// <returns>A new <see cref="Id"/> instance with a unique identifier.</returns>
    public static Id CreateUnique()
    {
        return new Id(Guid.NewGuid());
    }

    /// <summary>
    /// Creates a new instance of <see cref="MultiAnalysisId"/> with the specified identifier.
    /// </summary>
    /// <param name="guid">The unique identifier for the analysis.</param>
    /// <returns>A new <see cref="MultiAnalysisId"/> instance with the specified identifier.</returns>
    public static Id Create(Guid guid)
    {
        return new Id(guid);
    }
}
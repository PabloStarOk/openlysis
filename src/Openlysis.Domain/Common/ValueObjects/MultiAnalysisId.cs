namespace Openlysis.Domain.Common.ValueObjects;

public record MultiAnalysisId
{
    /// <summary>
    /// Gets the value of the <see cref="MultiAnalysisId"/>.
    /// </summary>
    public Guid Value { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="MultiAnalysisId"/> class with the specified value.
    /// </summary>
    /// <param name="value">The unique identifier for the analysis.</param>
    private MultiAnalysisId(Guid value)
    {
        Value = value;
    }

    /// <summary>
    /// Creates a new instance of <see cref="MultiAnalysisId"/> with a unique identifier.
    /// </summary>
    /// <returns>A new <see cref="MultiAnalysisId"/> instance with a unique identifier.</returns>
    public static MultiAnalysisId CreateUnique()
    {
        return new MultiAnalysisId(Guid.NewGuid());
    }

    /// <summary>
    /// Creates a new instance of <see cref="MultiAnalysisId"/> with the specified identifier.
    /// </summary>
    /// <param name="guid">The unique identifier for the analysis.</param>
    /// <returns>A new <see cref="MultiAnalysisId"/> instance with the specified identifier.</returns>
    public static MultiAnalysisId Create(Guid guid)
    {
        return new MultiAnalysisId(guid);
    }
}
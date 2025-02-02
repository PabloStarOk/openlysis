namespace Openlysis.Domain.FileAnalyses.ValueObjects;

/// <summary>
/// Represents the ID of a file report.
/// </summary>
public sealed record FileAnalysisId
{
    /// <summary>
    /// Gets the value.
    /// </summary>
    public Guid Value { get; }

    private FileAnalysisId(Guid value)
    {
        Value = value;
    }

    /// <summary>
    /// Creates a unique <see cref="FileAnalysisId"/>.
    /// </summary>
    /// <returns>A <see cref="FileAnalysisId"/>.</returns>
    public static FileAnalysisId CreateUnique()
    {
        return new FileAnalysisId(Guid.NewGuid());
    }
}
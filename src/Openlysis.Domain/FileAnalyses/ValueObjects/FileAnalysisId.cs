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

    /// <summary>
    /// Creates a <see cref="FileAnalysisId"/> from the specified <see cref="Guid"/>.
    /// </summary>
    /// <param name="guid">The <see cref="Guid"/> to create the <see cref="FileAnalysisId"/> from.</param>
    /// <returns>A <see cref="FileAnalysisId"/>.</returns>
    public static FileAnalysisId Create(Guid guid)
    {
        return new FileAnalysisId(guid);
    }
}
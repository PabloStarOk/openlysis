namespace Openlysis.Domain.FileAnalyses.ValueObjects;

/// <summary>
/// Represents the ID of a file report.
/// </summary>
public sealed record FileMultiAnalysisId
{
    /// <summary>
    /// Gets the value.
    /// </summary>
    public Guid Value { get; }

    private FileMultiAnalysisId(Guid value)
    {
        Value = value;
    }

    /// <summary>
    /// Creates a unique <see cref="FileMultiAnalysisId"/>.
    /// </summary>
    /// <returns>A <see cref="FileMultiAnalysisId"/>.</returns>
    public static FileMultiAnalysisId CreateUnique()
    {
        return new FileMultiAnalysisId(Guid.NewGuid());
    }

    /// <summary>
    /// Creates a <see cref="FileMultiAnalysisId"/> from the specified <see cref="Guid"/>.
    /// </summary>
    /// <param name="guid">The <see cref="Guid"/> to create the <see cref="FileMultiAnalysisId"/> from.</param>
    /// <returns>A <see cref="FileMultiAnalysisId"/>.</returns>
    public static FileMultiAnalysisId Create(Guid guid)
    {
        return new FileMultiAnalysisId(guid);
    }
}
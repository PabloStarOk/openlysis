namespace Openlysis.Domain.FileAnalyses.ValueObjects;

/// <summary>
/// Represents the ID of a file report.
/// </summary>
public sealed record ServiceAnalysisId
{
    /// <summary>
    /// Gets the value.
    /// </summary>
    public string Value { get; }

    private ServiceAnalysisId(string value)
    {
        Value = value;
    }

    /// <summary>
    /// Creates a <see cref="FileAnalysisId"/> from the specified <see cref="Guid"/>.
    /// </summary>
    /// <param name="id">A <see cref="string"/> to create the <see cref="FileAnalysisId"/> from.</param>
    /// <returns>A <see cref="ServiceAnalysisId"/>.</returns>
    public static ServiceAnalysisId Create(string id)
    {
        return new ServiceAnalysisId(id);
    }
}
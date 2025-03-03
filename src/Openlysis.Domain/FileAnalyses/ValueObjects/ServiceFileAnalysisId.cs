namespace Openlysis.Domain.FileAnalyses.ValueObjects;

/// <summary>
/// Represents a unique identifier for a service file analysis.
/// </summary>
public record ServiceFileAnalysisId
{
    /// <summary>
    /// Gets the value of the service file analysis identifier.
    /// </summary>
    public string Value { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="ServiceFileAnalysisId"/> class.
    /// </summary>
    /// <param name="value">The value of the service file analysis identifier.</param>
    private ServiceFileAnalysisId(string value)
    {
        Value = value;
    }

    /// <summary>
    /// Creates a new instance of the <see cref="ServiceFileAnalysisId"/> class.
    /// </summary>
    /// <param name="value">The value of the service file analysis identifier.</param>
    /// <returns>A new instance of the <see cref="ServiceFileAnalysisId"/> class.</returns>
    public static ServiceFileAnalysisId Create(string value)
    {
        return new ServiceFileAnalysisId(value);
    }
}
namespace Openlysis.Domain.Common.ValueObjects;

/// <summary>
/// Represents a unique identifier for an analysis of an external service.
/// </summary>
public record ServiceAnalysisId
{
    /// <summary>
    /// Gets the value of the ServiceAnalysisId.
    /// </summary>
    public string Value { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="ServiceAnalysisId"/> class.
    /// </summary>
    /// <param name="value">The value of the ServiceAnalysisId.</param>
    private ServiceAnalysisId(string value)
    {
        Value = value;
    }

    /// <summary>
    /// Creates a new instance of the <see cref="ServiceAnalysisId"/> class.
    /// </summary>
    /// <param name="value">The value of the ServiceAnalysisId.</param>
    /// <returns>A new instance of the <see cref="ServiceAnalysisId"/> class.</returns>
    public static ServiceAnalysisId Create(string value)
    {
        return new ServiceAnalysisId(value);
    }
}
namespace Openlysis.Domain.Common.ValueObjects;

/// <summary>
/// Ast
/// unique identifier for an analysis performed by an external service.
/// </summary>
public record AnalysisId
{
    /// <summary>
    /// Gets the value of the id.
    /// </summary>
    public string Value { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="AnalysisId"/> class.
    /// </summary>
    /// <param name="value">The value of the id.</param>
    private AnalysisId(string value)
    {
        Value = value;
    }

    /// <summary>
    /// Creates a new instance of the <see cref="AnalysisId"/> class.
    /// </summary>
    /// <param name="value">The value of the id.</param>
    /// <returns>A new instance of the <see cref="AnalysisId"/> class.</returns>
    public static AnalysisId Create(string value)
    {
        return new AnalysisId(value);
    }
}
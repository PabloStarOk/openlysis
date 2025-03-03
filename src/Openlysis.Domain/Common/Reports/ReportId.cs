namespace Openlysis.Domain.Common.Reports;

/// <summary>
/// Represents the ID of a report.
/// </summary>
public sealed record ReportId
{
    /// <summary>
    /// Gets the value.
    /// </summary>
    public string Value { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="ReportId"/> class.
    /// </summary>
    /// <param name="value">The value of the report ID.</param>
    private ReportId(string value)
    {
        Value = value;
    }

    /// <summary>
    /// Creates a new instance of the <see cref="ReportId"/> class.
    /// </summary>
    /// <param name="value">The value of the report ID.</param>
    /// <returns>A new instance of the <see cref="ReportId"/> class.</returns>
    public static ReportId Create(string value)
    {
        return new ReportId(value);
    }
}
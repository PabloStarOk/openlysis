namespace Openlysis.Domain.Common.Reports;

/// <summary>
/// Represents the ID of a report.
/// </summary>
public sealed record ReportId
{
    /// <summary>
    /// Gets the value.
    /// </summary>
    public Guid Value { get; }

    private ReportId(Guid value)
    {
        Value = value;
    }

    /// <summary>
    /// Creates a unique <see cref="ReportId"/>.
    /// </summary>
    /// <returns>A <see cref="ReportId"/>.</returns>
    public static ReportId CreateUnique()
    {
        return new ReportId(Guid.NewGuid());
    }
}
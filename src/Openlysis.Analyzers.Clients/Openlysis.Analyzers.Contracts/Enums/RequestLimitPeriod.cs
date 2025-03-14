namespace Openlysis.Analyzers.Contracts.Enums;

/// <summary>
/// Specifies the period for request limits.
/// </summary>
public enum RequestLimitPeriod
{
    /// <summary>
    /// Limit requests per minute.
    /// </summary>
    Minute,

    /// <summary>
    /// Limit requests per hour.
    /// </summary>
    Hour,

    /// <summary>
    /// Limit requests per day.
    /// </summary>
    Day,

    /// <summary>
    /// Limit requests per month.
    /// </summary>
    Month
}
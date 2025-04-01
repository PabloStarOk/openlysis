namespace Openlysis.Analyzers.Filescan.Core.Models.Requests;

/// <summary>
/// Represents a request to get a scan.
/// </summary>
/// <param name="FlowId">The ID of the flow.</param>
/// <param name="Filters">The filters to apply to the scan.</param>
/// <param name="Sorting">The sorting options for the scan results.</param>
/// <param name="OtherQueryParams">Other query parameters for the scan.</param>
public record GetScanRequest(
    string FlowId,
    string[]? Filters = null,
    string[]? Sorting = null,
    string[]? OtherQueryParams = null);
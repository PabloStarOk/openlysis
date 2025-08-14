namespace Openlysis.API.Endpoints.Users.GetAnalyses;

/// <summary>
/// Response record for retrieving a paginated list of analyses.
/// </summary>
/// <param name="Page">The current page number.</param>
/// <param name="PageSize">The number of items per page.</param>
/// <param name="Total">The total number of analyses available.</param>
/// <param name="Analyses">The collection of analysis objects.</param>
public record PaginatedAnalysisCollection(
    int Page,
    int PageSize,
    int Total,
    IEnumerable<object> Analyses);
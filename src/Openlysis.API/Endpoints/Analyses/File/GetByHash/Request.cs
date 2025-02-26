using FastEndpoints;

namespace Openlysis.API.Endpoints.Analyses.File.GetByHash;

/// <summary>
/// Represents a request to get a file analysis by hash.
/// </summary>
public record Request
{
    /// <summary>
    /// Gets the hash of the file.
    /// </summary>
    [RouteParam]
    public string Hash { get; init; } = string.Empty;

    /// <summary>
    /// Gets the amount of analyses to retrieve.
    /// </summary>
    [QueryParam]
    public int Amount { get; init; } = 10;

    /// <summary>
    /// Gets the order of the started date.
    /// </summary>
    [QueryParam]
    public string StartedDateOrder { get; init; } = string.Empty;
}
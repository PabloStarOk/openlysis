using FastEndpoints;

using Openlysis.Application.Common.Enums;

namespace Openlysis.API.Endpoints.Files.GetAnalysesByHash;

/// <summary>
/// Represents a request to get a file analysis by hash.
/// </summary>
public record GetAnalysesByHash
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
    public OrderType StartedDateOrder { get; init; } = OrderType.Dsc;
}
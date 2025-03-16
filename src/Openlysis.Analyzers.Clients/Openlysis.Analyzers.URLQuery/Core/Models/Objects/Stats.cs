using System.Text.Json.Serialization;

namespace Openlysis.Analyzers.URLQuery.Core.Models.Objects;

/// <summary>
/// Represents the statistics for urlquery alerts, IDS alerts, and threat detection systems alerts.
/// </summary>
/// <param name="UrlQueryAlerts">The number of urlquery alerts.</param>
/// <param name="IdsAlerts">The number of IDS alerts.</param>
/// <param name="ThreatDetectionSystemsAlerts">The number of threat detection systems alerts.</param>
public record Stats(
    [property: JsonPropertyName("urlquery")] int UrlQueryAlerts,
    [property: JsonPropertyName("ids")] int IdsAlerts,
    [property: JsonPropertyName("analyzer")] int ThreatDetectionSystemsAlerts);
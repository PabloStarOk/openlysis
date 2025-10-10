using System.Text.Json.Serialization;

using Openlysis.Analyzers.VirusTotal.Core.Models.Enums;

namespace Openlysis.Analyzers.VirusTotal.Core.Models.Objects;

/// <summary>
/// Attributes of the analysis.
/// </summary>
/// <param name="Stats">A <see cref="Stats"/> object containing the analysis statistics.</param>
/// <param name="Status">A <see cref="Status"/>.</param>
public record Attributes(
    [property: JsonPropertyName("stats")] Stats Stats,
    [property: JsonPropertyName("status")] Status Status);
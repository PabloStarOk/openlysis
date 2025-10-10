using System.Text.Json.Serialization;
using Openlysis.Analyzers.Filescan.Core.Models.Enums;

namespace Openlysis.Analyzers.Filescan.Core.Models.Objects;

/// <summary>
/// Represents the final verdict of Filescan service.
/// </summary>
/// <param name="Verdict">A <see cref="FilescanVerdict"/>.</param>
/// <param name="ThreatLevel">The threat level associated with the analysis.</param>
public record FinalVerdict(
    [property: JsonPropertyName("verdict")]
    FilescanVerdict? Verdict,
    [property: JsonPropertyName("threatLevel")]
    float? ThreatLevel)
{
    /// <summary>
    /// The maximum possible threat level value.
    /// </summary>
    public const float MaxPossibleThreatLevel = 1.0f;
}
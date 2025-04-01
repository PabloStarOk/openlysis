using System.Text.Json.Serialization;

namespace Openlysis.Analyzers.Filescan.Core.Models.Objects;

/// <summary>
/// Represents the report of an analysis of Filescan service.
/// </summary>
/// <param name="FinalVerdict">The final verdict of the Filescan analysis.</param>
public record FilescanReport(
    [property: JsonPropertyName("finalVerdict")] FinalVerdict? FinalVerdict);
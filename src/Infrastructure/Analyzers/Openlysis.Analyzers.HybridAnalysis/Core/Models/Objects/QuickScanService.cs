using System.Text.Json.Serialization;

namespace Openlysis.Analyzers.HybridAnalysis.Core.Models.Objects;

/// <summary>
/// Represents a service available for quick scanning in Hybrid Analysis.
/// </summary>
/// <param name="Name">The name of the scanning service.</param>
/// <param name="IsAvailable">Indicates whether the service is currently available.</param>
/// <param name="SupportedTypes">An array of file types supported by this scanning service.</param>
public record QuickScanService(
    string Name,
    [property: JsonPropertyName("available")] bool IsAvailable,
    [property: JsonPropertyName("supported_types")] string[] SupportedTypes);
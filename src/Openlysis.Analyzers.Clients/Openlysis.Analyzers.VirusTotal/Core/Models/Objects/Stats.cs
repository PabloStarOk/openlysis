using System.Text.Json.Serialization;

namespace Openlysis.Analyzers.VirusTotal.Core.Models.Objects;

/// <summary>
/// Represents the statistics of the analysis.
/// </summary>
/// <param name="Harmless">The number of harmless detections.</param>
/// <param name="Undetected">The number of undetected detections.</param>
/// <param name="Suspicious">The number of suspicious detections.</param>
/// <param name="Malicious">The number of malicious detections.</param>
/// <param name="Timeout">The number of timeouts.</param>
public record Stats(
    [property: JsonPropertyName("harmless")]
    int Harmless,
    [property: JsonPropertyName("undetected")]
    int Undetected,
    [property: JsonPropertyName("suspicious")]
    int Suspicious,
    [property: JsonPropertyName("malicious")]
    int Malicious,
    [property: JsonPropertyName("timeout")]
    int Timeout)
{
    /// <summary>
    /// Calculates the total number of detections.
    /// </summary>
    /// <returns>The total number of detections.</returns>
    public int GetTotal()
    {
        return Harmless + Undetected + Suspicious + Malicious + Timeout;
    }
}
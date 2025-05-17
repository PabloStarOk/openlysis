using System.Text.Json.Serialization;

using Openlysis.Analyzers.HybridAnalysis.Core.Models.Enums;

namespace Openlysis.Analyzers.HybridAnalysis.Core.Models.Responses;

/// <summary>
/// Represents a summary of an analysis performed by Hybrid Analysis.
/// </summary>
/// <param name="RawVerdict">The raw verdict string from the analysis result.</param>
/// <param name="ThreatScore">The numerical threat score assigned to the analyzed sample.</param>
internal record AnalysisSummary(
    [property: JsonPropertyName("verdict")] string? RawVerdict,
    float ThreatScore)
{
    /// <summary>
    /// Gets the verdict of the analysis as a <see cref="HybridAnalysisVerdict"/> enum.
    /// </summary>
    /// <remarks>
    /// The raw verdict string is trimmed and spaces are removed before parsing.
    /// </remarks>
    [JsonIgnore]
    internal HybridAnalysisVerdict Verdict
    {
        get
        {
            if (RawVerdict is null)
            {
                return HybridAnalysisVerdict.Unknown;
            }

            return Enum.Parse<HybridAnalysisVerdict>(
                RawVerdict
                    .Trim()
                    .Replace(" ", string.Empty),
                ignoreCase: true);
        }
    }
}
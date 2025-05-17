using System.Text.Json.Serialization;

using Openlysis.Analyzers.HybridAnalysis.Core.Models.Enums;

namespace Openlysis.Analyzers.HybridAnalysis.Core.Models.Responses;

/// <summary>
/// Represents a summary of a sandbox report.
/// </summary>
/// <param name="JobId">The job identifier.</param>
/// <param name="Status">The status of the job.</param>
/// <param name="RawVerdict">The verdict of the analysis as a string.</param>
/// <param name="ThreatScore">The threat score of the analysis.</param>
public record SandboxReportSummary(
    [property: JsonPropertyName("job_id")] string JobId,
    [property: JsonPropertyName("state")] Status Status,
    [property: JsonPropertyName("verdict")] string? RawVerdict,
    [property: JsonPropertyName("threat_score")] float? ThreatScore)
{
    /// <summary>
    /// Gets the verdict of the analysis as a <see cref="HybridAnalysisVerdict"/> enum.
    /// </summary>
    /// <remarks>
    /// The raw verdict string is trimmed and spaces are removed before parsing.
    /// </remarks>
    [JsonIgnore]
    public HybridAnalysisVerdict Verdict
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
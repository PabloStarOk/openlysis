using System.Text.Json.Serialization;

namespace Openlysis.Analyzers.HybridAnalysis.Core.Models.Responses;

/// <summary>
/// Represents the response received after submitting content for a sandbox analysis.
/// </summary>
/// <param name="JobId">The job identifier.</param>
public record SandboxSubmitResponse(
    [property: JsonPropertyName("job_id")] string JobId);
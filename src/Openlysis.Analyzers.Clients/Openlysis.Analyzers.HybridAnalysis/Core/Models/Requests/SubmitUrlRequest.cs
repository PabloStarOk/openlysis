using Openlysis.Analyzers.HybridAnalysis.Core.Models.Enums;

namespace Openlysis.Analyzers.HybridAnalysis.Core.Models.Requests;

/// <summary>
/// Represents a request to submit a URL for analysis.
/// </summary>
/// <param name="Url">The URL to be analyzed.</param>
/// <param name="SandboxEnvironment">The sandbox environment to use for the analysis.</param>
/// <param name="ExperimentalAntiEvasion">Indicates whether to use experimental anti-evasion techniques.</param>
public record SubmitUrlRequest(
    Uri Url,
    SandboxEnvironment SandboxEnvironment,
    bool ExperimentalAntiEvasion)
    : SandboxSubmitRequest<Uri>(Url, SandboxEnvironment, ExperimentalAntiEvasion);
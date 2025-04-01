using Openlysis.Analyzers.HybridAnalysis.Core.Models.Enums;

namespace Openlysis.Analyzers.HybridAnalysis.Core.Models.Requests;

/// <summary>
/// Represents a request to submit content to a sandbox analysis.
/// </summary>
/// <typeparam name="TContent">The type of the content to be submitted.</typeparam>
/// <param name="Content">The content to be submitted.</param>
/// <param name="SandboxEnvironment">The sandbox environment where the content will be analyzed.</param>
/// <param name="ExperimentalAntiEvasion">Indicates whether experimental anti-evasion techniques should be used.</param>
public abstract record SandboxSubmitRequest<TContent>(
    TContent Content,
    SandboxEnvironment SandboxEnvironment,
    bool ExperimentalAntiEvasion)
    where TContent : notnull;
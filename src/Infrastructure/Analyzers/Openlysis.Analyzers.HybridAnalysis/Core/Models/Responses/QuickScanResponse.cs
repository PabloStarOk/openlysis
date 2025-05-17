using System.Text.Json.Serialization;

namespace Openlysis.Analyzers.HybridAnalysis.Core.Models.Responses;

/// <summary>
/// Represents a response from a quick scan operation performed by Hybrid Analysis.
/// </summary>
/// <param name="Id">The unique identifier of the scan.</param>
/// <param name="Sha256">The SHA-256 hash of the scanned file.</param>
/// <param name="IsFinished">Indicates whether the scan has completed.</param>
internal record QuickScanResponse(
    string Id,
    string Sha256,
    [property: JsonPropertyName("finished")] bool IsFinished);
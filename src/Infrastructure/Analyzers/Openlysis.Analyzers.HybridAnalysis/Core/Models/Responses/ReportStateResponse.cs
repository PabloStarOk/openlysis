using System.Text.Json.Serialization;

using Openlysis.Analyzers.HybridAnalysis.Core.Models.Enums;

namespace Openlysis.Analyzers.HybridAnalysis.Core.Models.Responses;

/// <summary>
/// Represents the response containing the state of a report.
/// </summary>
/// <param name="Status">The current status of the report.</param>
/// <param name="ErrorType">The type of error, if any.</param>
/// <param name="ErrorOrigin">The origin of the error, if any.</param>
/// <param name="Error">The error message, if any.</param>
public record ReportStateResponse(
    [property: JsonPropertyName("state")] Status Status,
    [property: JsonPropertyName("error_type")] string ErrorType,
    [property: JsonPropertyName("error_origin")] string ErrorOrigin,
    [property: JsonPropertyName("error")] string Error);
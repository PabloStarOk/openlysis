using System.Text.Json.Serialization;

using Openlysis.Analyzers.URLQuery.Core.Models.Enums;

namespace Openlysis.Analyzers.URLQuery.Core.Models.Objects;

/// <summary>
/// Defines a base alert with a specified severity.
/// </summary>
/// <param name="Message">The alert message.</param>
/// <param name="Severity">The severity level of the alert.</param>
public record Alert(
    [property: JsonPropertyName("alert")] string Message,
    [property: JsonPropertyName("severity")] Severity Severity);
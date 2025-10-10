using System.Text.Json.Serialization;

namespace Openlysis.Analyzers.VirusTotal.Core.Models.Responses;

/// <summary>
/// Record representing a response containing a URL for uploading large files.
/// </summary>
/// <param name="Url">The URL provided by the API for file upload, mapped from "data" JSON property.</param>
internal record GetFileUploadUrl(
    [property: JsonPropertyName("data")] string Url);
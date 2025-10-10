using NJsonSchema;
using NJsonSchema.Annotations;

using Openlysis.Application.Common.Models;
using Openlysis.Domain.Common.ValueObjects;

namespace Openlysis.API.Endpoints.Files.Analyze;

/// <summary>
/// Represents a request to analyze a file, including the file data, password, privacy setting, and reanalysis flag.
/// </summary>
/// <param name="File">The processed file to be analyzed.</param>
/// <param name="Password">The password associated with the file.</param>
/// <param name="IsPrivate">Indicates whether the analysis is private. Defaults to <see cref="DefaultIsPrivate"/>.</param>
/// <param name="Reanalyze">Indicates whether to reanalyze the file. Defaults to <see cref="DefaultReanalyze"/>.</param>
public sealed record AnalyzeFileRequest(
    [property: JsonSchemaIgnore] GlobalId UserId,
    [property: JsonSchema(JsonObjectType.File)] ProcessedFile File,
    string Password,
    bool IsPrivate = true,
    bool Reanalyze = false)
{
    /// <summary>
    /// The default value indicating the analysis is private.
    /// </summary>
    public const bool DefaultIsPrivate = true;

    /// <summary>
    /// The default value indicating the file should not be reanalyzed.
    /// </summary>
    public const bool DefaultReanalyze = false;
}
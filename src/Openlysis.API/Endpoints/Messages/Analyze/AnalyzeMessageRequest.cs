using NJsonSchema;
using NJsonSchema.Annotations;

using Openlysis.Application.Common.Models;
using Openlysis.Domain.Common.ValueObjects;
using Openlysis.Domain.Messages.Enums;

namespace Openlysis.API.Endpoints.Messages.Analyze;

/// <summary>
/// Represents a request to analyze a message.
/// </summary>
/// <param name="MessageType">The type of the message being analyzed.</param>
/// <param name="Sender">The sender of the message.</param>
/// <param name="Content">The content of the message.</param>
/// <param name="Subject">The optional subject of the message.</param>
/// <param name="AttachedFiles">The collection of files attached to the message.</param>
/// <param name="AttachedFilesPasswords">A dictionary containing passwords for the attached files, if any.</param>
/// <param name="CountryCode">The optional country code to improve data detection in the message.</param>
/// <param name="IsPrivate">Indicates whether the message analysis is private and non-accessible for other users.</param>
/// <param name="Reanalyze">Specifies whether to reanalyze the message even if there is an existing analysis available to retrieve.</param>
public record AnalyzeMessageRequest(
    [property: JsonSchemaIgnore] GlobalId UserId,
    MessageType? MessageType,
    string Sender,
    string Content,
    string? Subject,
    [property: JsonSchema(JsonObjectType.Array, Format = "file")]
    ProcessedFile[] AttachedFiles,
    Dictionary<string, string> AttachedFilesPasswords,
    string? CountryCode = null,
    bool IsPrivate = true,
    bool Reanalyze = false)
{
    /// <summary>
    /// The default value indicating that message analysis is private.
    /// </summary>
    public const bool DefaultIsPrivate = true;

    /// <summary>
    /// The default value indicating that reanalysis is not performed.
    /// </summary>
    public const bool DefaultReanalyze = false;

    /// <summary>
    /// Gets the normalized country code in uppercase format, or null if no country code is provided.
    /// </summary>
    public string? NormalizedCountryCode => CountryCode?.ToUpper();
}
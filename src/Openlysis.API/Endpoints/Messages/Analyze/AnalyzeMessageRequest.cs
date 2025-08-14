using System.Security.Claims;

using FastEndpoints;

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
/// <param name="IsPrivate">Indicates whether the message analysis is private and non-accessible for other users.</param>
/// <param name="Reanalyze">Specifies whether to reanalyze the message even if there is an existing analysis available to retrieve.</param>
/// <param name="CountryCode">The optional country code to improve data detection in the message.</param>
public record AnalyzeMessageRequest(
    [property: FromClaim(
        ClaimType = ClaimTypes.NameIdentifier,
        RemoveFromSchema = true)]
    string UserId,
    MessageType? MessageType,
    string Sender,
    string Content,
    string? Subject,
    IFormFileCollection? AttachedFiles,
    Dictionary<string, string>? AttachedFilesPasswords,
    bool IsPrivate = true,
    bool Reanalyze = false,
    string? CountryCode = null)
{
    /// <summary>
    /// Gets the normalized country code in uppercase format, or null if no country code is provided.
    /// </summary>
    public string? NormalizedCountryCode => CountryCode?.ToUpper();
}
namespace Openlysis.API.Endpoints.Emails.Analyze;

/// <summary>
/// Represents a request to analyze an email message.
/// </summary>
/// <param name="Sender">The sender of the email message.</param>
/// <param name="Subject">The optional subject of the email message.</param>
/// <param name="Content">The content of the email message.</param>
/// <param name="AttachedFiles">The collection of files attached to the email message.</param>
/// <param name="AttachedFilesPasswords">A dictionary containing passwords for the attached files, if any.</param>
/// <param name="IsPrivate">Indicates whether the message analysis is private and non-accessible for other users.</param>
/// <param name="ReanalyzeData">Specifies whether to analyze detected data in the email even if there are existing analysis results for them.</param>
/// <param name="CountryCode">The optional country code to improve data detection in the email.</param>
public record AnalyzeEmailRequest(
    string Sender,
    string Content,
    string? Subject,
    IFormFileCollection? AttachedFiles,
    Dictionary<string, string>? AttachedFilesPasswords,
    bool IsPrivate = true,
    bool ReanalyzeData = false,
    string? CountryCode = null)
{
    /// <summary>
    /// Gets the normalized country code in uppercase format, or null if no country code is provided.
    /// </summary>
    public string? NormalizedCountryCode => CountryCode?.ToUpper();
}
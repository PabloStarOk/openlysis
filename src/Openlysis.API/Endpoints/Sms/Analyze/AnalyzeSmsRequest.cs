namespace Openlysis.API.Endpoints.Sms.Analyze;

/// <summary>
/// Represents a request to analyze an SMS message.
/// </summary>
/// <param name="Sender">The sender of the SMS message.</param>
/// <param name="Content">The content of the SMS message.</param>
/// <param name="IsPrivate">Indicates whether the message analysis is private and non-accessible for other users.</param>
/// <param name="ReanalyzeData">Specifies whether to analyze detected data in the SMS even if there are existing analysis results for them.</param>
/// <param name="CountryCode">The optional country code to improve data detection in the SMS.</param>
public record AnalyzeSmsRequest(
    string Sender,
    string Content,
    bool IsPrivate = true,
    bool ReanalyzeData = false,
    string? CountryCode = null)
{
    /// <summary>
    /// Gets the normalized country code in uppercase format, or null if no country code is provided.
    /// </summary>
    public string? NormalizedCountryCode => CountryCode?.ToUpper();
}
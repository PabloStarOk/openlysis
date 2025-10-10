namespace Openlysis.API.Endpoints.Users.GetAnalyses;

/// <summary>
/// Specifies the type of analysis that can be retrieved.
/// </summary>
public enum AnalysisType
{
    /// <summary>
    /// Analysis of a URL.
    /// </summary>
    Url,

    /// <summary>
    /// Analysis of a file.
    /// </summary>
    File,

    /// <summary>
    /// Analysis of an email message.
    /// </summary>
    Email,

    /// <summary>
    /// Analysis of an SMS message.
    /// </summary>
    Sms,
}
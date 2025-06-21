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
    /// Analysis of a message.
    /// </summary>
    Message,
}
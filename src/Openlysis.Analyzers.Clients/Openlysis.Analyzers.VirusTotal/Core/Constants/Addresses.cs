namespace Openlysis.Analyzers.VirusTotal.Core.Constants;

/// <summary>
/// Contains the base address and endpoints for the VirusTotal API.
/// </summary>
public static class Addresses
{
    /// <summary>
    /// The base address for the VirusTotal API.
    /// </summary>
    public const string BaseAddress = "https://www.virustotal.com/api/v3/";

    /// <summary>
    /// The endpoint for URL submissions.
    /// </summary>
    public const string UrlsEndpoint = "urls";

    /// <summary>
    /// The endpoint for analysis results.
    /// </summary>
    public const string AnalysesEndpoint = "analyses/{0,0}";
}
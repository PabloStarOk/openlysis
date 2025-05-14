namespace Openlysis.Analyzers.VirusTotal.Core.Constants;

/// <summary>
/// Contains the base address and endpoints for the VirusTotal API.
/// </summary>
public static class Addresses
{
    /// <summary>
    /// The endpoint for small file submissions (files under 32MB).
    /// </summary>
    public const string SmallFilesEndpoint = "files";

    /// <summary>
    /// The endpoint for obtaining an upload URL for large files.
    /// </summary>
    public const string FileUploadUrlEndpoint = "files/upload_url";

    /// <summary>
    /// The endpoint for URL submissions.
    /// </summary>
    public const string UrlsEndpoint = "urls";

    /// <summary>
    /// The endpoint for analysis results.
    /// </summary>
    public const string AnalysesEndpoint = "analyses/{0,0}";
}
namespace Openlysis.Analyzers.Filescan.Core.Constants;

/// <summary>
/// Contains the addresses used in the Filescan client.
/// </summary>
internal static class Addresses
{
    /// <summary>
    /// The base address of the Filescan API.
    /// </summary>
    internal const string BaseAddress = "https://www.filescan.io";

    /// <summary>
    /// The endpoint for scanning files.
    /// </summary>
    internal const string ScanFile = "/api/scan/file";

    /// <summary>
    /// The endpoint for scanning URLs.
    /// </summary>
    internal const string ScanUrl = "/api/scan/url";

    /// <summary>
    /// The endpoint for get all reports of a scan.
    /// </summary>
    internal const string GetScan = "/api/scan/{0,0}/report";
}
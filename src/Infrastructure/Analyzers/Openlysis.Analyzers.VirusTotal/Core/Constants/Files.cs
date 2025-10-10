namespace Openlysis.Analyzers.VirusTotal.Core.Constants;

/// <summary>
/// Contains constants related to file size limitations and thresholds
/// used by the VirusTotal analyzer.
/// </summary>
internal static class Files
{
    /// <summary>
    /// The maximum size in bytes for small files (32MB).
    /// Files smaller than or equal to this size will be processed using the regular file endpoint.
    /// </summary>
    internal const long SmallFilesMaxSizeInBytes = 33553369;
}
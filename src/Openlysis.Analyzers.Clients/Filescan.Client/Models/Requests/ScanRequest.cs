using Filescan.Client.Models.Scans;

namespace Filescan.Client.Models.Requests;

/// <summary>
/// Represents a request to scan a file.
/// </summary>
/// <param name="FileName">The name of the file to be scanned.</param>
/// <param name="FileMimeType">The MIME type of the file to upload.</param>
/// <param name="FileData">The stream containing the file data.</param>
/// <param name="Description">An optional description of the file.</param>
/// <param name="Tags">Optional tags associated with the file.</param>
/// <param name="PropagateTags">Indicates whether tags should be propagated.</param>
/// <param name="Password">An optional password for the file.</param>
/// <param name="IsPrivateFile">Indicates whether the file is private.</param>
/// <param name="IsPrivateReport">Indicates whether the report is private.</param>
/// <param name="SkipWhiteListed">Indicates whether to skip whitelisted items.</param>
/// <param name="ScanProfile">The scan profile to be used.</param>
/// <param name="Options">A <see cref="ScanOptions"/> to configure the scan.</param>
public record ScanRequest(
    string FileName,
    string FileMimeType,
    Stream FileData,
    string Description = "",
    string[]? Tags = null,
    bool? PropagateTags = null,
    string Password = "",
    bool? IsPrivateFile = null,
    bool? IsPrivateReport = null,
    bool? SkipWhiteListed = null,
    string ScanProfile = "",
    ScanOptions? Options = null);
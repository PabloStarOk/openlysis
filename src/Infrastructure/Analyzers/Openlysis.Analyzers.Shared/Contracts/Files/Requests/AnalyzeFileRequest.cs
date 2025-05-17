using Openlysis.Analyzers.Shared.Contracts.Common.Requests;

namespace Openlysis.Analyzers.Shared.Contracts.Files.Requests;

/// <summary>
/// Represents a request to analyze a file.
/// </summary>
/// <param name="FileData">The stream containing the file data.</param>
/// <param name="CreateFileDataStreamAsync">A function that creates a new stream of the file data asynchronously.</param>
/// <param name="FileName">The name of the file.</param>
/// <param name="FileContentType">The content type of the file.</param>
/// <param name="FilePassword">The password for the file, if any.</param>
/// <param name="FileSha256">The SHA-256 hash of the file.</param>
/// <param name="IsPrivateFile">Indicates whether the file is private.</param>
public record AnalyzeFileRequest(
    Stream FileData,
    Func<CancellationToken, Task<Stream>> CreateFileDataStreamAsync,
    string FileName,
    string FileContentType,
    string FilePassword,
    string FileSha256,
    bool IsPrivateFile)
    : AnalyzeRequest(string.Empty, IsPrivateFile);
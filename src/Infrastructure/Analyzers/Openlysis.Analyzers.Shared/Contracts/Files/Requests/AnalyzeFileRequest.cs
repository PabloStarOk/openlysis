using Openlysis.Analyzers.Shared.Contracts.Common.Requests;
using Openlysis.Analyzers.Shared.Infrastructure.Files.Abstractions;

namespace Openlysis.Analyzers.Shared.Contracts.Files.Requests;

/// <summary>
/// Represents a request to analyze a file, including all necessary metadata and a factory for accessing its content.
/// </summary>
/// <remarks>
/// This record encapsulates all information required to analyze a file, such as its stream factory, name, content type,
/// optional password, SHA-256 hash, and privacy status.
/// </remarks>
/// <param name="StreamFactory">
/// The <see cref="IStreamFactory"/> used to asynchronously create a <see cref="Stream"/> for accessing the file's content.
/// </param>
/// <param name="FileName">
/// The name of the file to be analyzed.
/// </param>
/// <param name="FileContentType">
/// The MIME type of the file.
/// </param>
/// <param name="FileSize">
/// The size of the file in bytes.
/// </param>
/// <param name="FilePassword">
/// The password for the file, if it is protected; otherwise, <c>null</c>.
/// </param>
/// <param name="FileSha256">
/// The SHA-256 hash of the file for integrity verification.
/// </param>
/// <param name="IsPrivateFile">
/// Indicates whether the file is private and should be handled accordingly.
/// </param>
public record AnalyzeFileRequest(
    IStreamFactory StreamFactory,
    string FileName,
    string FileContentType,
    long FileSize,
    string FilePassword,
    string FileSha256,
    bool IsPrivateFile)
    : AnalyzeRequest(string.Empty, IsPrivateFile);
using Openlysis.Domain.Common.ValueObjects;
using Openlysis.Infrastructure.Shared.Communication.Abstractions;

namespace Openlysis.Infrastructure.Shared.Communication.Contracts;

/// <summary>
/// Represents a file to be analyzed, including metadata and security information.
/// </summary>
/// <param name="MultiAnalysisId">The global identifier for the multi-analysis operation.</param>
/// <param name="Filename">The name of the file to analyze.</param>
/// <param name="FileContentType">The MIME type of the file.</param>
/// <param name="FileSha256">The SHA-256 hash of the file content.</param>
/// <param name="FilePassword">The password for the file, if it is protected.</param>
/// <param name="IsPrivateFile">Indicates whether the file is private.</param>
/// <param name="FileInstanceId">The unique identifier for this file to use with <see cref="IFileStorageProvider"/>.</param>
public sealed record AnalyzeFile(
    GlobalId MultiAnalysisId,
    string Filename,
    string FileContentType,
    string FileSha256,
    string FilePassword,
    bool IsPrivateFile,
    string FileInstanceId);
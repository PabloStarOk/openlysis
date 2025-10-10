using Openlysis.Domain.Common.ValueObjects;

namespace Openlysis.Infrastructure.Shared.Communication.Contracts;

/// <summary>
/// Represents a message to request a file analysis job within a multi-analysis context.
/// </summary>
/// <param name="MultiAnalysisId">The global identifier for the multi-analysis operation.</param>
/// <param name="Filename">The name of the file to analyze.</param>
/// <param name="FileContentType">The MIME type of the file.</param>
/// <param name="FileSize">The size of the file in bytes.</param>
/// <param name="FileSha256">The SHA-256 hash of the file content.</param>
/// <param name="FilePassword">The password for the file, if it is protected.</param>
/// <param name="IsPrivateFile">Indicates whether the file is private.</param>
/// <param name="StorageFileName">The name for this file in the storage.</param>
/// <param name="CorrelationId">Optional correlation identifier that associates the request to a message analysis.</param>
public sealed record FileAnalysisJobMessage(
    GlobalId MultiAnalysisId,
    string Filename,
    string FileContentType,
    long FileSize,
    string FileSha256,
    string FilePassword,
    bool IsPrivateFile,
    string StorageFileName,
    GlobalId? CorrelationId = null)
    : AnalysisJobMessage(MultiAnalysisId, CorrelationId);
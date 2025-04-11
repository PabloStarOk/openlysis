using Openlysis.Domain.Common.ValueObjects;

namespace Openlysis.MultiAnalyzer.Core.Broker.Files;

/// <summary>
/// Represents a request to start a job to analyze a file.
/// </summary>
/// <param name="FileMultiAnalysisId">The ID of the multi analysis of a file.</param>
/// <param name="FileName">The name of the file.</param>
/// <param name="FileContentType">The content type of the file.</param>
/// <param name="FileId">The unique identifier of the file.</param>
/// <param name="FileDescription">A description of the file.</param>
/// <param name="FilePassword">The password for the file, if any.</param>
/// <param name="IsPrivateFile">Indicates if the file is private.</param>
public record AnalyzeFile(
    GlobalId FileMultiAnalysisId,
    string FileName,
    string FileContentType,
    string FileId,
    string FileDescription,
    string FilePassword,
    bool IsPrivateFile);
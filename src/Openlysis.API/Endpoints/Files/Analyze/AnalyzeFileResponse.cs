namespace Openlysis.API.Endpoints.Files.Analyze;

/// <summary>
/// Response for analyze file endpoint.
/// </summary>
/// <param name="FileAnalysisId">The unique identifier for the file analysis.</param>
/// <param name="FileAnalysisMd5">The MD5 hash of the file analysis.</param>
/// <param name="FileAnalysisSha1">The SHA-1 hash of the file analysis.</param>
/// <param name="FileAnalysisSha256">The SHA-256 hash of the file analysis.</param>
/// <param name="FileAnalysisSha512">The SHA-512 hash of the file analysis.</param>
public record AnalyzeFileResponse(
    string FileAnalysisId,
    string FileAnalysisMd5,
    string FileAnalysisSha1,
    string FileAnalysisSha256,
    string FileAnalysisSha512);
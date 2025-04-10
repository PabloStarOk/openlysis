namespace Openlysis.Analyzers.Shared.Core.Files.Requests;

/// <summary>
/// Represents a request to analyze a file.
/// </summary>
/// <param name="FileData">The stream containing the file data.</param>
/// <param name="FileName">The name of the file.</param>
/// <param name="FileContentType">The content type of the file.</param>
/// <param name="FileDescription">A description of the file.</param>
/// <param name="FilePassword">The password for the file, if any.</param>
/// <param name="IsPrivateFile">Indicates whether the file is private.</param>
public record FileAnalysisRequest(
    Stream FileData,
    string FileName,
    string FileContentType,
    string FileDescription,
    string FilePassword,
    bool IsPrivateFile);
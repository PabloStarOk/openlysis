namespace Openlysis.Application.Common.Interfaces.Ports;

/// <summary>
/// Represents a request to start a job to analyze a file.
/// </summary>
/// <param name="FileName">The name of the file.</param>
/// <param name="FileContentType">The content type of the file.</param>
/// <param name="FileStreamData">The stream data of the file.</param>
/// <param name="Description">A description of the file.</param>
/// <param name="Password">The password for the file, if any.</param>
/// <param name="IsPrivateFile">Indicates if the file is private.</param>
public record FileAnalysisJobRequest(
    string FileName,
    string FileContentType,
    Stream FileStreamData,
    string Description,
    string Password,
    bool IsPrivateFile);
namespace Openlysis.API.Endpoints.Files.Analyze;

/// <summary>
/// Request to analyze a file.
/// </summary>
/// <param name="File">File to analyze</param>
/// <param name="FileDescription">Description of the file</param>
/// <param name="FilePassword">Password for the file if it is protected</param>
/// <param name="IsPrivateFile">Indicates if the file is private</param>
/// <param name="Reanalyze">If file has already been analyzed by another user, reanalyze it again.</param>
public record AnalyzeFileRequest(IFormFile File,
    string FileDescription = "",
    string FilePassword = "",
    bool IsPrivateFile = true,
    bool Reanalyze = false);
namespace Openlysis.API.Endpoints.Files.Analyze;

/// <summary>
/// Request to analyze a file.
/// </summary>
/// <param name="File">File to analyze</param>
/// <param name="Password">Password for the file if it is protected</param>
/// <param name="IsPrivate">Indicates if the file multi analysis should be private.</param>
/// <param name="Reanalyze">If file has already been analyzed by another user, reanalyze it again.</param>
public record AnalyzeFileRequest(
    IFormFile? File,
    string Password = "",
    bool IsPrivate = true,
    bool Reanalyze = false);
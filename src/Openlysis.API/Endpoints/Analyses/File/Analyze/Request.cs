namespace Openlysis.API.Endpoints.Analyses.File.Analyze;

/// <summary>
/// Request to analyze a file.
/// </summary>
/// <param name="File">File to analyze</param>
/// <param name="Reanalyze">If file has already been analyzed by another user, reanalyze it again.</param>
public record Request(IFormFile File, bool Reanalyze);
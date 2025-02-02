using ErrorOr;

using MediatR;

using Openlysis.Domain.FileReports.ValueObjects;

namespace Openlysis.Application.FileAnalyses.Commands;

/// <summary>
/// Command to analyze a file.
/// </summary>
/// <param name="FileData">Data of the file to be analyzed.</param>
public record AnalyzeFileCommand(
    string Filename,
    DateTime CreationDate,
    byte[] FileData,
    bool Reanalyze = false)
    : IRequest<ErrorOr<FileAnalysisId>>;
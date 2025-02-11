using ErrorOr;

using MediatR;

using Openlysis.Domain.FileAnalyses;

namespace Openlysis.Application.FileAnalyses.Commands;

/// <summary>
/// Command to analyze a file.
/// </summary>
/// <param name="FileData">Data of the file to be analyzed.</param>
public record AnalyzeFileCommand(
    string FileName,
    string FileContentType,
    Stream FileData,
    bool Reanalyze = false)
    : IRequest<ErrorOr<FileAnalysis>>;
using ErrorOr;

using MediatR;

using Openlysis.Domain.Files;
using Openlysis.Domain.Users.ValueObjects;

namespace Openlysis.Application.Files.Commands;

/// <summary>
/// Command to analyze a file.
/// </summary>
/// <param name="UserId">The ID of the user initiating the analysis.</param>
/// <param name="FileName">The name of the file.</param>
/// <param name="FileContentType">The content type of the file.</param>
/// <param name="FileData">The data stream of the file.</param>
/// <param name="FileDescription">A description of the file.</param>
/// <param name="FilePassword">The password for the file, if any.</param>
/// <param name="IsPrivateFile">Indicates whether the file is private.</param>
/// <param name="Reanalyze">Indicates whether to reanalyze the file, if another user has already analyzed it.</param>
/// <returns>A result containing the file analysis or an error.</returns>
public record AnalyzeFileCommand(
    UserId UserId,
    string FileName,
    string FileContentType,
    Stream FileData,
    string FileDescription,
    string FilePassword,
    bool IsPrivateFile,
    bool Reanalyze)
    : IRequest<ErrorOr<FileMultiAnalysis>>;
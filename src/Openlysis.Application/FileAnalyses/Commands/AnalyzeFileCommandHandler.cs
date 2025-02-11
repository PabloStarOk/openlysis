using ErrorOr;

using MediatR;

using Openlysis.Application.Common.Interfaces.Persistence;
using Openlysis.Application.Common.Interfaces.Services;
using Openlysis.Application.FileAnalyses.Ports;
using Openlysis.Domain.FileAnalyses;
using Openlysis.Domain.FileAnalyses.Enums;
using Openlysis.Domain.FileAnalyses.ValueObjects;

namespace Openlysis.Application.FileAnalyses.Commands;

/// <summary>
/// Handles <see cref="AnalyzeFileCommand"/>.
/// </summary>
public class AnalyzeFileCommandHandler : IRequestHandler<AnalyzeFileCommand, ErrorOr<FileAnalysis>>
{
    private readonly IFileAnalysisRepository _fileAnalysisRepository;
    private readonly IEnumerable<IFileAnalyzer> _fileAnalyzers;
    private readonly TimeProvider _timeProvider;
    private readonly IHashService _hashService;

    /// <summary>
    /// Initializes a new instance of the <see cref="AnalyzeFileCommandHandler"/> class.
    /// </summary>
    /// <param name="fileAnalyzers">Analyzers services.</param>
    /// <param name="timeProvider">Provider of time.</param>
    /// <param name="fileAnalysisRepository">Repository of file analyses.</param>
    /// <param name="hashService">Service to hash data.</param>
    public AnalyzeFileCommandHandler(
        IFileAnalysisRepository fileAnalysisRepository,
        IEnumerable<IFileAnalyzer> fileAnalyzers,
        TimeProvider timeProvider,
        IHashService hashService)
    {
        _fileAnalysisRepository = fileAnalysisRepository;
        _fileAnalyzers = fileAnalyzers;
        _timeProvider = timeProvider;
        _hashService = hashService;
    }

    /// <inheritdoc/>
    public async Task<ErrorOr<FileAnalysis>> Handle(AnalyzeFileCommand command, CancellationToken cancellationToken)
    {
        var hashSet = await _hashService.HashDataAsync(command.FileData, cancellationToken);

        // Check if the file has already been analyzed.
        var existingAnalysis = await _fileAnalysisRepository.GetByHashAsync(hashSet);
        if (existingAnalysis is not null)
        {
            if (command.Reanalyze)
            {
                await AnalyzeFileAsync(command.FileData, cancellationToken);
            }

            return existingAnalysis;
        }

        await AnalyzeFileAsync(command.FileData, cancellationToken);

        // Save the new file analysis in database.
        var fileMetadata = new FileMetadata(
            command.FileName,
            command.FileContentType,
            command.FileData.Length,
            hashSet);
        var fileAnalysis = FileAnalysis.Create(_timeProvider.GetUtcNow().DateTime, Verdict.Undetected, fileMetadata, []);
        await _fileAnalysisRepository.AddAsync(fileAnalysis);
        return fileAnalysis;
    }

    private async Task AnalyzeFileAsync(Stream fileData, CancellationToken cancellationToken)
    {
        var list = _fileAnalyzers.Select(f => f.AnalyzeAsync(fileData, cancellationToken));
        await Task.WhenAll(list);
    }
}
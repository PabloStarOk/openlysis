using ErrorOr;

using MediatR;

using Openlysis.Application.Common.Interfaces.Persistence;
using Openlysis.Application.Common.Interfaces.Services;
using Openlysis.Application.FileAnalyses.Ports;
using Openlysis.Domain.FileAnalyses;
using Openlysis.Domain.FileAnalyses.ValueObjects;

namespace Openlysis.Application.FileAnalyses.Commands;

/// <summary>
/// Handles <see cref="AnalyzeFileCommand"/>.
/// </summary>
public class AnalyzeFileCommandHandler : IRequestHandler<AnalyzeFileCommand, ErrorOr<FileMultiAnalysis>>
{
    private readonly IFileMultiAnalysisRepository _fileMultiAnalysisRepository;
    private readonly IEnumerable<IFileAnalyzer> _fileAnalyzers;
    private readonly TimeProvider _timeProvider;
    private readonly IHashService _hashService;

    /// <summary>
    /// Initializes a new instance of the <see cref="AnalyzeFileCommandHandler"/> class.
    /// </summary>
    /// <param name="fileAnalyzers">Analyzers services.</param>
    /// <param name="timeProvider">Provider of time.</param>
    /// <param name="fileMultiAnalysisRepository">Repository of file analyses.</param>
    /// <param name="hashService">Service to hash data.</param>
    public AnalyzeFileCommandHandler(
        IFileMultiAnalysisRepository fileMultiAnalysisRepository,
        IEnumerable<IFileAnalyzer> fileAnalyzers,
        TimeProvider timeProvider,
        IHashService hashService)
    {
        _fileMultiAnalysisRepository = fileMultiAnalysisRepository;
        _fileAnalyzers = fileAnalyzers;
        _timeProvider = timeProvider;
        _hashService = hashService;
    }

    /// <inheritdoc/>
    public async Task<ErrorOr<FileMultiAnalysis>> Handle(AnalyzeFileCommand command, CancellationToken cancellationToken)
    {
        var hashSet = await _hashService.HashDataAsync(command.FileData, cancellationToken);

        // Check if the file has already been analyzed.
        var existingAnalysis = await _fileMultiAnalysisRepository.GetByHashAsync(hashSet);
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
            command.FileContentType,
            command.FileData.Length,
            hashSet);
        var fileAnalysis = FileMultiAnalysis.Create(_timeProvider.GetUtcNow().DateTime, command.FileName, fileMetadata, []);
        await _fileMultiAnalysisRepository.AddAsync(fileAnalysis);
        return fileAnalysis;
    }

    private async Task AnalyzeFileAsync(Stream fileData, CancellationToken cancellationToken)
    {
        var list = _fileAnalyzers.Select(f => f.AnalyzeAsync(fileData, cancellationToken));
        await Task.WhenAll(list);
    }
}
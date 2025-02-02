using ErrorOr;

using FileSignatures;

using MediatR;

using Openlysis.Application.Common.Interfaces.Persistence;
using Openlysis.Application.Common.Interfaces.Services;
using Openlysis.Application.FileAnalyses.Ports;
using Openlysis.Domain.Common.Hash;
using Openlysis.Domain.FileReports;
using Openlysis.Domain.FileReports.Enums;
using Openlysis.Domain.FileReports.ValueObjects;

using File = Openlysis.Domain.FileReports.ValueObjects.File;

namespace Openlysis.Application.FileAnalyses.Commands;

/// <summary>
/// Handles <see cref="AnalyzeFileCommand"/>.
/// </summary>
public class AnalyzeFileCommandHandler : IRequestHandler<AnalyzeFileCommand, ErrorOr<FileAnalysisId>>
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
    public async Task<ErrorOr<FileAnalysisId>> Handle(AnalyzeFileCommand command, CancellationToken cancellationToken)
    {
        HashSet hashSet;
        using (var dataMemoryStream = new MemoryStream(command.FileData))
        {
            hashSet = await _hashService.HashDataAsync(dataMemoryStream);
        }

        // Check if the file has already been analyzed.
        var existingAnalysis = await _fileAnalysisRepository.GetByHashAsync(hashSet);
        if (existingAnalysis is not null)
        {
            if (command.Reanalyze)
            {
                await AnalyzeFileAsync(command.FileData, cancellationToken);
            }

            return existingAnalysis.Id;
        }

        await AnalyzeFileAsync(command.FileData, cancellationToken);

        // Save the new file analysis in database.
        var fileFormatInspector = new FileFormatInspector();
        var mimeType = "other";

        using (var stream = new MemoryStream(command.FileData))
        {
            var format = fileFormatInspector.DetermineFileFormat(stream);

            if (format is not null)
            {
                mimeType = format.MediaType;
            }
        }

        var fileGeneralInfo = new FileGeneralInfo(command.Filename, mimeType, command.FileData.Length, command.CreationDate);
        var fileInfo = new File(hashSet, fileGeneralInfo);
        var fileAnalysis = FileAnalysis.Create(_timeProvider.GetUtcNow().DateTime, Verdict.Undetected, fileInfo, []);
        await _fileAnalysisRepository.AddAsync(fileAnalysis);
        return fileAnalysis.Id;
    }

    private async Task AnalyzeFileAsync(byte[] fileData, CancellationToken cancellationToken)
    {
        var list = _fileAnalyzers.Select(f => f.AnalyzeAsync(fileData, cancellationToken));
        await Task.WhenAll(list);
    }
}
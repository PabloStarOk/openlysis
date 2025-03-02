using ErrorOr;

using MediatR;

using Openlysis.Application.Common.Interfaces.Persistence;
using Openlysis.Application.Common.Interfaces.Services;
using Openlysis.Domain.FileAnalyses;
using Openlysis.Domain.FileAnalyses.ValueObjects;

namespace Openlysis.Application.FileAnalyses.Commands;

/// <summary>
/// Handles <see cref="AnalyzeFileCommand"/>.
/// </summary>
public class AnalyzeFileCommandHandler : IRequestHandler<AnalyzeFileCommand, ErrorOr<FileMultiAnalysis>>
{
    private readonly IRepository<FileMultiAnalysis, FileMultiAnalysisId> _fileMultiAnalysisRepository;
    private readonly TimeProvider _timeProvider;
    private readonly IHashService _hashService;
    private readonly IFileMultiAnalysisService _multiAnalysisService;

    /// <summary>
    /// Initializes a new instance of the <see cref="AnalyzeFileCommandHandler"/> class.
    /// </summary>
    /// <param name="fileMultiAnalysisRepository">Repository of file analyses.</param>
    /// <param name="multiAnalysisService">Service to analyze the file.</param>
    /// <param name="timeProvider">Provider of time.</param>
    /// <param name="hashService">Service to hash data.</param>
    public AnalyzeFileCommandHandler(
        IRepository<FileMultiAnalysis, FileMultiAnalysisId> fileMultiAnalysisRepository,
        IFileMultiAnalysisService multiAnalysisService,
        TimeProvider timeProvider,
        IHashService hashService)
    {
        _fileMultiAnalysisRepository = fileMultiAnalysisRepository;
        _timeProvider = timeProvider;
        _hashService = hashService;
        _multiAnalysisService = multiAnalysisService;
    }

    /// <inheritdoc/>
    public async Task<ErrorOr<FileMultiAnalysis>> Handle(AnalyzeFileCommand command, CancellationToken cancellationToken)
    {
        var hashSet = await _hashService.HashDataAsync(command.FileData, cancellationToken);

        // Check if the file has already been analyzed.
        var existingAnalyses = await _fileMultiAnalysisRepository.GetManyAsync(
            1,
            f => f.ContentHashSet == hashSet,
            q => q.OrderByDescending(f => f.StartedDate),
            cancellationToken);

        if (existingAnalyses.Count > 0 && !command.Reanalyze)
        {
            return existingAnalyses[0];
        }

        // Save a new file analysis in database.
        var fileMetadata = new FileMetadata(
            command.FileName,
            command.FileContentType,
            command.FileData.Length);
        var multiAnalysis = FileMultiAnalysis.Create(
            _timeProvider.GetUtcNow().DateTime,
            fileMetadata,
            hashSet,
            []);

        await _multiAnalysisService.StartAnalysisAsync(
            multiAnalysis,
            command.FileData,
            command.FileDescription,
            command.FilePassword,
            command.IsPrivateFile,
            cancellationToken);

        return multiAnalysis;
    }
}
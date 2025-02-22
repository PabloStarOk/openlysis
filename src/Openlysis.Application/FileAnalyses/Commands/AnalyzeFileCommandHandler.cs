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
        var existingAnalysis = await _fileMultiAnalysisRepository.GetByHashAsync(hashSet, cancellationToken);

        if (existingAnalysis is not null && !command.Reanalyze)
        {
            return existingAnalysis;
        }

        await AnalyzeFileAsync(command, cancellationToken);

        // Save a new file analysis in database.
        var fileMetadata = new FileMetadata(
            command.FileName,
            command.FileContentType,
            command.FileData.Length);
        var fileAnalysis = FileMultiAnalysis.Create(
            _timeProvider.GetUtcNow().DateTime,
            fileMetadata,
            hashSet,
            []);
        await _fileMultiAnalysisRepository.AddAsync(fileAnalysis, cancellationToken);
        return fileAnalysis;
    }

    /// <summary>
    /// Analyzes the file using all available analyzers.
    /// </summary>
    /// <param name="command">The file analysis command containing file data and metadata.</param>
    /// <param name="cancellationToken">Token to monitor for cancellation requests.</param>
    private async Task AnalyzeFileAsync(AnalyzeFileCommand command, CancellationToken cancellationToken)
    {
        var list = _fileAnalyzers.Select(f =>
            {
                command.FileData.Position = 0;
                var request = new AnalyzeFileRequest(
                    command.FileName,
                    command.FileContentType,
                    command.FileData,
                    Description: string.Empty, // TODO: Get description from endpoints.
                    Password: string.Empty, // TODO: Get password from endpoints.
                    IsPrivateFile: true); // TODO: Get option from endpoints, but prefer true.
                return f.AnalyzeAsync(request, cancellationToken);
            });
        await Task.WhenAll(list);
    }
}
using System.Collections.Generic;
using System.Threading.Channels;
using System.Threading.Tasks;

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

using Openlysis.AnalysisOrchestrator.Abstractions;
using Openlysis.AnalysisOrchestrator.Configuration;
using Openlysis.Analyzers.Shared.Contracts.Common.Abstractions;
using Openlysis.Analyzers.Shared.Contracts.Files.Requests;
using Openlysis.Analyzers.Shared.Infrastructure.Files.Services;
using Openlysis.Domain.Files.Entities;
using Openlysis.Infrastructure.Shared.Communication.Abstractions;
using Openlysis.Infrastructure.Shared.Communication.Contracts;

namespace Openlysis.AnalysisOrchestrator.Infrastructure;

/// <summary>
/// Analyzes files using multiple analyzers, handling analysis jobs and requests.
/// </summary>
internal sealed class FileMultiAnalyzer : MultiAnalyzer<FileAnalysis, FileAnalysisJobMessage, AnalyzeFileRequest>
{
    private readonly IFileStorageProvider _fileStorageProvider;
    private FileStreamFactory _fileStreamFactory;
    private string _storageFileName;

    /// <summary>
    /// Initializes a new instance of the <see cref="FileMultiAnalyzer"/> class.
    /// </summary>
    /// <param name="logger">Logger instance for logging analyzer activity.</param>
    /// <param name="options">Options for configuring the multi-analyzer.</param>
    /// <param name="fileStorageProvider">Provider for file storage operations.</param>
    /// <param name="analyzers">Dictionary of available analyzers keyed by name.</param>
    /// <param name="channel">Channel for processing file analysis results.</param>
    public FileMultiAnalyzer(
        ILogger<FileMultiAnalyzer> logger,
        IOptions<MultiAnalyzerOptions> options,
        IFileStorageProvider fileStorageProvider,
        IReadOnlyDictionary<string, Analyzer<FileAnalysis, AnalyzeFileRequest>> analyzers,
        Channel<FileAnalysis> channel)
        : base(logger, options, analyzers, channel)
    {
        _fileStorageProvider = fileStorageProvider;
    }

    /// <inheritdoc/>
    protected override AnalyzeFileRequest CreateRequest(FileAnalysisJobMessage message)
    {
        _storageFileName = message.StorageFileName;
        _fileStreamFactory = new FileStreamFactory(message.StorageFileName, _fileStorageProvider);
        return new AnalyzeFileRequest(
            _fileStreamFactory,
            message.Filename,
            message.FileContentType,
            message.FileSize,
            message.FilePassword,
            message.FileSha256,
            message.IsPrivateFile);
    }

    /// <inheritdoc/>
    protected override async ValueTask CleanUpAsync(bool success)
    {
        if (_fileStreamFactory is not null)
        {
            await _fileStreamFactory.DisposeAsync();
        }

        if (success)
        {
            await _fileStorageProvider.DeleteAsync(_storageFileName);
        }
    }
}
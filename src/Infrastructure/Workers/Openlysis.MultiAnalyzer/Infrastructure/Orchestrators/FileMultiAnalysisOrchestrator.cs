using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using ErrorOr;

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

using Openlysis.Analyzers.Shared.Contracts.Common.Abstractions;
using Openlysis.Analyzers.Shared.Contracts.Files.Requests;
using Openlysis.Analyzers.Shared.Infrastructure.Files.Services;
using Openlysis.Domain.Common.Enums;
using Openlysis.Domain.Files.Entities;
using Openlysis.Infrastructure.Shared.Communication.Abstractions;
using Openlysis.Infrastructure.Shared.Communication.Contracts;
using Openlysis.MultiAnalyzer.Abstractions;
using Openlysis.MultiAnalyzer.Configuration;

namespace Openlysis.MultiAnalyzer.Infrastructure.Orchestrators;

/// <summary>
/// Orchestrates the analysis of files using multiple analyzers.
/// Inherits from <see cref="MultiAnalysisOrchestrator{FileAnalysis, AnalyzeFile}"/>.
/// </summary>
internal sealed class FileMultiAnalysisOrchestrator
    : MultiAnalysisOrchestrator<FileAnalysis, AnalyzeFileMessage>
{
    private readonly Dictionary<string, Analyzer<FileAnalysis, AnalyzeFileRequest>> _analyzers;
    private readonly IFileStorageProvider _fileStorageProvider;

    /// <summary>
    /// Initializes a new instance of the <see cref="FileMultiAnalysisOrchestrator"/> class.
    /// </summary>
    /// <param name="logger">The logger instance for logging orchestration events.</param>
    /// <param name="options">The orchestration options.</param>
    /// <param name="fileStorageProvider">The file storage provider for file operations.</param>
    /// <param name="updateMessageSender">The message sender for updating multi-analysis status.</param>
    /// <param name="analyzers">The collection of analyzers to use for file analysis.</param>
    public FileMultiAnalysisOrchestrator(
        ILogger<FileMultiAnalysisOrchestrator> logger,
        IOptions<OrchestrationOptions> options,
        IFileStorageProvider fileStorageProvider,
        IUpdateMessageSender<UpdateMultiAnalysisMessage<FileAnalysis>> updateMessageSender,
        IEnumerable<Analyzer<FileAnalysis, AnalyzeFileRequest>> analyzers)
        : base(logger, options, updateMessageSender)
    {
        _fileStorageProvider = fileStorageProvider;
        _analyzers = analyzers.ToDictionary(a => a.ServiceName);
    }

    /// <inheritdoc/>
    protected override async Task<IEnumerable<FileAnalysis>> HandleAnalyzeAsync(
        AnalyzeFileMessage message,
        CancellationToken cancellationToken)
    {
        MultiAnalysisId = message.MultiAnalysisId;

        await using var fileStreamFactory = new FileStreamFactory(
            message.StorageFileName,
            _fileStorageProvider);

        var analyzeRequest = new AnalyzeFileRequest(
            fileStreamFactory,
            message.Filename,
            message.FileContentType,
            message.FileSize,
            message.FilePassword,
            message.FileSha256,
            message.IsPrivateFile);

        ConcurrentBag<FileAnalysis> analyses = [];
        await Parallel.ForEachAsync(
            _analyzers.Values,
            cancellationToken,
            async (analyzer, _) =>
            {
                if (!analyzer.CanAnalyze)
                {
                    Logger.LogWarning("Trying to analyze using an unavailable service.");
                    return;
                }

                var analyzeResult = await analyzer.AnalyzeAsync(
                    analyzeRequest,
                    cancellationToken);
                if (analyzeResult.IsError)
                {
                    Logger.LogError(
                        "One or more errors occurred while analyzing a file: {Errors}",
                        analyzeResult.Errors);
                    return;
                }

                analyses.Add(analyzeResult.Value);
            });

        await _fileStorageProvider.DeleteAsync(
            message.StorageFileName,
            cancellationToken);
        return analyses;
    }

    /// <inheritdoc/>
    protected override async ValueTask UpdateAnalysisAsync(
        FileAnalysis pendingAnalysis,
        CancellationToken cancellationToken)
    {
        var analyzer = _analyzers[pendingAnalysis.ExternalId.Service];
        if (!analyzer.CanGetAnalysisStatus)
        {
            Logger.LogDebug("Trying to get analysis status when service is unavailable.");
            return;
        }

        // Get status
        ErrorOr<AnalysisStatus> getStatusResult =
            await analyzer.GetStatusAsync(pendingAnalysis.ExternalId, cancellationToken);
        if (getStatusResult.IsError)
        {
            pendingAnalysis.UpdateStatus(AnalysisStatus.Failed);
            PendingAnalyses.TryRemove(pendingAnalysis.Id, out _);
            UpdatableAnalyses.Add(pendingAnalysis);
            return;
        }

        // Update status
        if (getStatusResult.Value
            is AnalysisStatus.Queued
            or AnalysisStatus.InProgress)
        {
            return;
        }

        if (!analyzer.CanGetAnalysis)
        {
            Logger.LogDebug("Trying to get analysis when service is unavailable.");
            return;
        }

        // Get full analysis
        ErrorOr<FileAnalysis> getAnalysisResult = await analyzer.GetAnalysisAsync(
            pendingAnalysis.ExternalId,
            cancellationToken);
        if (getAnalysisResult.IsError)
        {
            return;
        }

        FileAnalysis updatedAnalysis = getAnalysisResult.Value;
        UpdateAnalysisNewValues(pendingAnalysis, updatedAnalysis);
        PendingAnalyses.TryRemove(pendingAnalysis.Id, out _);
        UpdatableAnalyses.Add(pendingAnalysis);
    }

    /// <summary>
    /// Updates the values of <paramref name="currentAnalysis"/> with those from <paramref name="newAnalysis"/>.
    /// Copies verdict, threat score, status, and synchronizes reports.
    /// </summary>
    private static void UpdateAnalysisNewValues(
        FileAnalysis currentAnalysis,
        FileAnalysis newAnalysis)
    {
        currentAnalysis.UpdateVerdict(newAnalysis.State.Verdict);
        currentAnalysis.UpdateThreatScore(newAnalysis.ThreatScore);
        foreach (var report in newAnalysis.Reports)
        {
            if (currentAnalysis.Reports.Contains(report))
            {
                currentAnalysis.UpdateReport(report);
            }
            else
            {
                currentAnalysis.AddReport(report);
            }
        }

        currentAnalysis.UpdateStatus(newAnalysis.State.Status);
    }

#if DEBUG
    /// <inheritdoc/>
    protected override void LogUpdateMessage(
        UpdateMultiAnalysisMessage<FileAnalysis> message)
    {
        Logger.LogTrace(
            "Sending update multi analysis message:"
            + "\n\tID: {Id}"
            + "\n\tIs timeout: {Timeout}",
            message.MultiAnalysisId,
            message.Timeout);
    }

    /// <inheritdoc/>
    protected override void LogUpdatableAnalysis(FileAnalysis analysis)
    {
        Logger.LogTrace(
            "Sending updatable analysis:"
            + "\n\tID: {Id}"
            + "\n\tExternal ID: {ExternalId}"
            + "\n\tVerdict: {Verdict}"
            + "\n\tThreat score: {ThreatScore}"
            + "\n\tStatus: {Status}"
            + "\n\tReports count: {ReportsCount}",
            analysis.Id,
            analysis.ExternalId,
            analysis.State.Verdict,
            analysis.ThreatScore,
            analysis.State.Status,
            analysis.Reports.Count);

        foreach (var report in analysis.Reports)
        {
            LogReport(report);
        }
    }

    /// <summary>
    /// Logs detailed information about a <see cref="FileReport"/> using trace level logging.
    /// </summary>
    /// <param name="report">The file report to log.</param>
    private void LogReport(FileReport report)
    {
        Logger.LogTrace(
            "Report:"
            + "\n\tID: {Id}"
            + "\n\tExternal ID: {ExternalId}"
            + "\n\tVerdict: {Verdict}"
            + "\n\tThreat score: {ThreatScore}",
            report.Id,
            report.ExternalId,
            report.Verdict,
            report.ThreatScore);
    }
#endif
}
using Microsoft.Extensions.Logging;

using Openlysis.Domain.Common.Constants;
using Openlysis.Domain.Common.Enums;
using Openlysis.Domain.Files.Entities;
using Openlysis.TestTools.ServicesSimulation.Common.Configuration;
using Openlysis.TestTools.ServicesSimulation.Common.Enums;
using Openlysis.TestTools.ServicesSimulation.Common.Services.Analyzers;
using Openlysis.TestTools.ServicesSimulation.Files.Configuration;

namespace Openlysis.TestTools.ServicesSimulation.Files.Infrastructure;

/// <summary>
/// Builder class for creating file analysis stubs used in testing scenarios.
/// </summary>
/// <remarks>
/// Extends the <see cref="AnalysisStubBuilder{TOptions, TAnalysis}"/> class with implementations
/// specific to file analysis simulations.
/// </remarks>
internal sealed class FileAnalysisStubBuilder
    : AnalysisStubBuilder<FileAnalysisStubFactoryOptions, FileServiceAnalysis>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="FileAnalysisStubBuilder"/> class.
    /// </summary>
    /// <param name="logger">The logger used to record diagnostic information during stub building operations.</param>
    public FileAnalysisStubBuilder(ILogger<FileAnalysisStubBuilder> logger)
        : base(logger)
    {
    }

    /// <inheritdoc/>
    protected override FileServiceAnalysis HandleCreation(
        string serviceName,
        FileAnalysisStubFactoryOptions options)
    {
        return FileServiceAnalysis.Create(
            Guid.NewGuid().ToString(),
            serviceName,
            AnalysisStatus.Queued,
            Verdict.Unknown,
            jobId: GenerateJobId(options));
    }

    /// <inheritdoc/>
    protected override void HandleFinalization(
        FileServiceAnalysis analysis,
        FileAnalysisStubFactoryOptions options)
    {
        if (options.UseFileReports)
        {
            UpdateWithReports(analysis, options);
            return;
        }

        Verdict verdict = GenerateVerdict(options.VerdictSimulation);
        analysis.UpdateVerdict(verdict);

        float threatScore = GenerateThreatScore(options.ThreatScoreSimulation);
        analysis.UpdateThreatScore(threatScore);

        AnalysisStatus status = GenerateAnalysisStatus(options.StatusSimulation);
        analysis.UpdateStatus(status);
    }

    /// <inheritdoc/>
    protected override void LogCreatedStub(FileServiceAnalysis stub)
    {
        Logger.LogDebug($"{nameof(FileServiceAnalysis)} created in an initial state.");
    }

    /// <inheritdoc/>
    protected override void LogFinalizedAnalysis(FileServiceAnalysis analysis)
    {
        IEnumerable<string> reportAsStrings = analysis.Reports
            .Select(r => $"{{ "
                + $"\n\tID: {r.Id}"
                + $"\n\tVerdict: {r.Verdict}"
                + $"\n\tThreat Score: {r.ThreatScore}}}");
        string reportsLog = string.Join(Environment.NewLine, reportAsStrings);

        Logger.LogTrace(
            "{TypeName} created:"
            + "\n\tService name: {ServiceName}"
            + "\n\tID: {Id}"
            + "\n\tVerdict: {Verdict}"
            + "\n\tStatus: {Status}"
            + "\n\tThreat score: {ThreatScore}"
            + "\n\tReports amount: {ReportsAmount}"
            + "\n\tReports: {Reports}",
            typeof(FileServiceAnalysis),
            analysis.ServiceName,
            analysis.Id,
            analysis.State.Verdict,
            analysis.State.Status,
            analysis.ThreatScore,
            analysis.Reports.Count,
            reportsLog);
    }

    /// <summary>
    /// Updates the analysis with file reports generated based on the provided options.
    /// </summary>
    /// <param name="analysis">The file service analysis to update with reports.</param>
    /// <param name="options">The configuration options that define how reports should be generated.</param>
    private static void UpdateWithReports(
        FileServiceAnalysis analysis,
        FileAnalysisStubFactoryOptions options)
    {
        IEnumerable<FileReport> fileReports = GenerateFileReports(options);
        foreach (var report in fileReports)
        {
            analysis.AddReport(report);
        }

        AnalysisStatus status = GenerateAnalysisStatus(options.StatusSimulation);
        analysis.UpdateStatus(status);
    }

    /// <summary>
    /// Creates a new instance of <see cref="FileReportStubOptions"/> with random simulation settings.
    /// </summary>
    /// <returns>A new <see cref="FileReportStubOptions"/> instance configured with random verdict and threat score simulations.</returns>
    private static FileReportStubOptions CreateRandomReportStubOptions()
    {
        var verdictSimulation = new VerdictOptions
        {
            SimulationType = SimulationType.Random,
        };

        var threatScoreSimulation = new ThreatScoreOptions
        {
            SimulationType = SimulationType.Random,
        };

        return new FileReportStubOptions
        {
            VerdictSimulation = verdictSimulation,
            ThreatScoreSimulation = threatScoreSimulation,
        };
    }

    /// <summary>
    /// Generates a randomized file report based on the provided stub options.
    /// </summary>
    /// <param name="stubOptions">Configuration options that determine how the file report should be generated.</param>
    /// <returns>A new <see cref="FileReport"/> instance with randomized properties based on the simulation settings.</returns>
    private static FileReport GenerateRandomFileReport(FileReportStubOptions stubOptions)
    {
        Verdict verdict = GenerateVerdict(stubOptions.VerdictSimulation);
        float threatScore = GenerateThreatScore(stubOptions.ThreatScoreSimulation);
        return FileReport.Create(
            Guid.NewGuid().ToString(),
            verdict,
            ThreatZoneMapping.Map[verdict],
            threatScore);
    }

    /// <summary>
    /// Generates a collection of file reports based on the provided factory options.
    /// </summary>
    /// <param name="options">The configuration options that determine how many file reports to generate and their characteristics.</param>
    /// <returns>A collection of <see cref="FileReport"/> instances generated according to the specified options.</returns>
    private static List<FileReport> GenerateFileReports(
        FileAnalysisStubFactoryOptions options)
    {
        List<FileReport> reports = [];

        for (int i = 0; i < options.MaxFileReports; i++)
        {
            FileReportStubOptions stubOptions =
                options.ReturnableFileReports.Length is 0
                ? CreateRandomReportStubOptions()
                : GetRandomValue(options.ReturnableFileReports);

            FileReport report = GenerateRandomFileReport(stubOptions);
            reports.Add(report);
        }

        return reports;
    }
}
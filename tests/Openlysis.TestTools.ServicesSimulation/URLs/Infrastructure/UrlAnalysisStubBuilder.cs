using Microsoft.Extensions.Logging;

using Openlysis.Domain.Common.Enums;
using Openlysis.Domain.URLs.Entities;
using Openlysis.TestTools.ServicesSimulation.Common.Configuration;
using Openlysis.TestTools.ServicesSimulation.Common.Services.Analyzers;

namespace Openlysis.TestTools.ServicesSimulation.URLs.Infrastructure;

/// <summary>
/// Builds stub implementations of URL analysis services for testing purposes.
/// </summary>
/// <remarks>
/// This class is responsible for creating and finalizing URL service analysis stubs
/// with configurable behaviors based on provided options.
/// </remarks>
internal sealed class UrlAnalysisStubBuilder
    : AnalysisStubBuilder<AnalysisStubFactoryOptions, UrlServiceAnalysis>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="UrlAnalysisStubBuilder"/> class.
    /// </summary>
    /// <param name="logger">The logger used for diagnostic information.</param>
    public UrlAnalysisStubBuilder(ILogger<UrlAnalysisStubBuilder> logger)
        : base(logger)
    {
    }

    /// <inheritdoc/>
    protected override void HandleFinalization(
        UrlServiceAnalysis analysis,
        AnalysisStubFactoryOptions options)
    {
        Verdict verdict = GenerateVerdict(options.VerdictSimulation);
        analysis.UpdateVerdict(verdict);

        float threatScore = GenerateThreatScore(options.ThreatScoreSimulation);
        analysis.UpdateThreatScore(threatScore);

        AnalysisStatus status = GenerateAnalysisStatus(options.StatusSimulation);
        analysis.UpdateStatus(status);
    }

    /// <inheritdoc/>
    protected override UrlServiceAnalysis HandleCreation(
        string serviceName,
        AnalysisStubFactoryOptions options)
    {
        return UrlServiceAnalysis.Create(
            Guid.NewGuid().ToString(),
            serviceName,
            AnalysisStatus.Queued,
            Verdict.Unknown,
            GenerateJobId(options));
    }

    /// <inheritdoc/>
    protected override void LogCreatedStub(UrlServiceAnalysis stub)
    {
        Logger.LogDebug($"{nameof(UrlServiceAnalysis)} created in an initial state.");
    }

    /// <inheritdoc/>
    protected override void LogFinalizedAnalysis(UrlServiceAnalysis analysis)
    {
        Logger.LogTrace(
            "{TypeName} created:"
            + "\n\tService name: {ServiceName}"
            + "\n\tID: {Id}"
            + "\n\tVerdict: {Verdict}"
            + "\n\tThreat score: {ThreatScore}"
            + "\n\tStatus: {Status}",
            nameof(UrlServiceAnalysis),
            analysis.ServiceName,
            analysis.Id,
            analysis.State.Verdict,
            analysis.ThreatScore.ToString() ?? "null",
            analysis.State.Status);
    }
}
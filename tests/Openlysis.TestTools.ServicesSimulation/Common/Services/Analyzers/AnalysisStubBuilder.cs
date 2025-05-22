using Microsoft.Extensions.Logging;

using Openlysis.Domain.Common.Entities;
using Openlysis.Domain.Common.Enums;
using Openlysis.TestTools.ServicesSimulation.Common.Configuration;
using Openlysis.TestTools.ServicesSimulation.Common.Enums;
using Openlysis.TestTools.ServicesSimulation.Common.Models;
using Openlysis.TestTools.ServicesSimulation.Common.Services.Abstractions;

namespace Openlysis.TestTools.ServicesSimulation.Common.Services.Analyzers;

/// <summary>
/// Represents a factory for creating analysis stubs with specific options.
/// </summary>
/// <typeparam name="TOptions">The type of options for creating analysis stubs.</typeparam>
/// <typeparam name="TAnalysisStub">The type of analysis stub to create.</typeparam>
internal abstract class AnalysisStubBuilder<TOptions, TAnalysisStub>
    : StubFactory<TOptions, TAnalysisStub>
    where TOptions : AnalysisStubFactoryOptions
    where TAnalysisStub : ServiceAnalysis
{
    /// <summary>
    /// Initializes a new instance of the <see cref="AnalysisStubBuilder{TOptions, TAnalysisStub}"/> class.
    /// </summary>
    /// <param name="logger">The logger instance for logging information and errors.</param>
    protected AnalysisStubBuilder(
        ILogger<AnalysisStubBuilder<TOptions, TAnalysisStub>> logger)
        : base(logger)
    {
    }

    /// <summary>
    /// Finalizes the analysis stub with the specified options and logs the result.
    /// </summary>
    /// <param name="analysis">The analysis stub to be finalized.</param>
    /// <param name="options">The options to be applied during finalization.</param>
    internal void Finalize(
        TAnalysisStub analysis,
        TOptions options)
    {
        HandleFinalization(analysis, options);
        LogFinalizedAnalysis(analysis);
    }

    /// <summary>
    /// Generates an analysis status based on the provided options.
    /// </summary>
    /// <param name="statusOptions">Configuration options that determine how the analysis status is generated.</param>
    /// <returns>An AnalysisStatus value calculated according to the specified simulation type.</returns>
    protected static AnalysisStatus GenerateAnalysisStatus(
        AnalysisStatusOptions statusOptions)
    {
        IReadOnlyList<AnalysisStatus> possibleRandomStatuses = Enum
            .GetValues<AnalysisStatus>()
            .Where(s => s is not AnalysisStatus.Queued and not AnalysisStatus.InProgress)
            .ToList();

        return statusOptions.SimulationType switch
        {
            SimulationType.Random => GetRandomValue(possibleRandomStatuses),
            SimulationType.Fixed => statusOptions.FixedValue,
            SimulationType.Range => throw new InvalidOperationException("Range of values is not supported for enum simulation."),
            SimulationType.Set => GetRandomValue(statusOptions.ValuesSet.ToList()),
            _ => throw new InvalidOperationException($"Unsupported value for {nameof(statusOptions.SimulationType)}"),
        };
    }

    /// <summary>
    /// Generates a threat score based on the provided options.
    /// </summary>
    /// <param name="scoreOptions">Configuration options that determine how the threat score is generated.</param>
    /// <returns>A float representing the threat score calculated according to the specified simulation type.</returns>
    protected static float GenerateThreatScore(ThreatScoreOptions scoreOptions)
    {
        return scoreOptions.SimulationType switch
        {
            SimulationType.Random => GenerateRandomThreatScore(),
            SimulationType.Fixed => scoreOptions.FixedValue,
            SimulationType.Range => GenerateRandomThreatScoreFromRange(scoreOptions.ValuesRange),
            SimulationType.Set => GetRandomValue(scoreOptions.ValuesSet.ToList()),
            _ => throw new InvalidOperationException($"Unsupported value for {nameof(scoreOptions.SimulationType)}"),
        };
    }

    /// <summary>
    /// Handles the finalization process for the analysis stub with the specified options.
    /// </summary>
    /// <param name="analysis">The analysis stub to be finalized.</param>
    /// <param name="options">The options that control how the finalization is performed.</param>
    protected abstract void HandleFinalization(
        TAnalysisStub analysis,
        TOptions options);

    /// <summary>
    /// Outputs debug information about the finalized analysis.
    /// </summary>
    /// <param name="analysis">The analysis stub that has been finalized.</param>
    /// <remarks>This method is only compiled in debug builds.</remarks>
    protected abstract void LogFinalizedAnalysis(TAnalysisStub analysis);

    /// <summary>
    /// Generates a random threat score between 0 and 1.
    /// </summary>
    /// <returns>A random float value between 0 and 1.</returns>
    private static float GenerateRandomThreatScore()
    {
        var range = new SimulationRange<float>
        {
            Minimum = 0,
            Maximum = 1,
        };
        return GenerateRandomThreatScoreFromRange(range);
    }

    /// <summary>
    /// Generates a random threat score within the specified range.
    /// </summary>
    /// <param name="range">The range containing minimum and maximum values for the threat score.</param>
    /// <returns>A random float value between the minimum and maximum values of the range.</returns>
    /// <exception cref="ArgumentNullException">Thrown when the range parameter is null.</exception>
    private static float GenerateRandomThreatScoreFromRange(
        SimulationRange<float>? range)
    {
        ArgumentNullException.ThrowIfNull(range);

        const int mantissaBits = 23;
        const int maxMantissa = 1 << mantissaBits;
        long m = Random.Shared.NextInt64(0, maxMantissa + 1);
        return m / (float)maxMantissa;
    }
}
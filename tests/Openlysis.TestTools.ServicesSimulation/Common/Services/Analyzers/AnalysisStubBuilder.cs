using Microsoft.Extensions.Logging;

using Openlysis.Domain.Common.Entities;
using Openlysis.Domain.Common.Enums;
using Openlysis.Domain.Common.ValueObjects;
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
    where TAnalysisStub : Analysis
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
    /// Generates a job ID based on the provided options.
    /// </summary>
    /// <param name="options">The options that determine whether a job ID should be generated.</param>
    /// <returns>A new GUID as a string if options.ReturnJobId is true; otherwise, null.</returns>
    protected static string? GenerateJobId(TOptions options)
    {
        string? jobId = null;
        if (options.ReturnJobId)
        {
            jobId = Guid.NewGuid().ToString();
        }

        return jobId;
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
    protected static ThreatScore GenerateThreatScore(ThreatScoreOptions scoreOptions)
    {
        return scoreOptions.SimulationType switch
        {
            SimulationType.Random => GenerateRandomThreatScore(),
            SimulationType.Fixed => ThreatScore.Create(
                scoreOptions.FixedValue,
                scoreOptions.MaxPossibleValue),
            SimulationType.Range => GenerateRandomThreatScoreFromRange(
                    scoreOptions.ValuesRange,
                    scoreOptions.MaxPossibleValue),
            SimulationType.Set => GenerateThreatScoreFromSet(scoreOptions),
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
    private static ThreatScore GenerateRandomThreatScore()
    {
        const int maxPossibleValue = 100;
        var range = new SimulationRange<float>
        {
            Minimum = 0,
            Maximum = maxPossibleValue,
        };
        return GenerateRandomThreatScoreFromRange(range, maxPossibleValue);
    }

    /// <summary>
    /// Generates a random <see cref="ThreatScore"/> within the specified range and maximum possible value.
    /// </summary>
    /// <param name="range">The range of possible float values for the threat score.</param>
    /// <param name="maxPossibleValue">The maximum possible value for the threat score.</param>
    /// <returns>A <see cref="ThreatScore"/> generated from a random value within the given range.</returns>
    private static ThreatScore GenerateRandomThreatScoreFromRange(
        SimulationRange<float>? range,
        float maxPossibleValue)
    {
        ArgumentNullException.ThrowIfNull(range);

        const int multiplier = 10_000_000;
        long scaledMin = (long)(multiplier * range.Minimum);
        long scaledMax = (long)(multiplier * range.Maximum);
        long random = Random.Shared.NextInt64(scaledMin, scaledMax + 1);
        float randomThreatScore = random / (float)multiplier;
        return ThreatScore.Create(randomThreatScore, maxPossibleValue);
    }

    /// <summary>
    /// Generates a threat score by selecting a random value from the provided set of possible values.
    /// </summary>
    /// <param name="options">The options containing the set of possible threat score values and the maximum possible value.</param>
    /// <returns>A <see cref="ThreatScore"/> created from a randomly selected value in the set.</returns>
    private static ThreatScore GenerateThreatScoreFromSet(ThreatScoreOptions options)
    {
        float value = GetRandomValue(options.ValuesSet.ToList());
        return ThreatScore.Create(value, options.MaxPossibleValue);
    }
}
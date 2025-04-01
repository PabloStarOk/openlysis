using Openlysis.Analyzers.VirusTotal.Core.Models.Objects;
using Openlysis.Domain.Common.Enums;

namespace Openlysis.Analyzers.VirusTotal.Core.Abstractions;

/// <summary>
/// Interface for calculating verdicts based on detection statistics.
/// </summary>
public interface IVerdictCalculator
{
    /// <summary>
    /// Calculates a verdict based on the provided detection statistics.
    /// </summary>
    /// <param name="detectionStats">The statistics of the detection to base the verdict on.</param>
    /// <returns>A <see cref="Verdict"/> representing the calculated verdict.</returns>
    public Verdict Calculate(Stats detectionStats);
}
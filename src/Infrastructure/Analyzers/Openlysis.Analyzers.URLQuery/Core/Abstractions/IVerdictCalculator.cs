using Openlysis.Analyzers.URLQuery.Core.Models.Objects;
using Openlysis.Domain.Common.Enums;

namespace Openlysis.Analyzers.URLQuery.Core.Abstractions;

/// <summary>
/// Interface for calculating verdicts based on alert statistics.
/// </summary>
public interface IVerdictCalculator
{
    /// <summary>
    /// Calculates a verdict based on the provided alert statistics.
    /// </summary>
    /// <param name="sensors">The statistics of the sensors to base the verdict on.</param>
    /// <returns>A <see cref="Verdict"/> representing the calculated verdict.</returns>
    public Verdict Calculate(Sensors sensors);
}
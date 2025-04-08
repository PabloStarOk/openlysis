using Openlysis.Domain.Common.Enums;

namespace Openlysis.Assessors.Shared.Abstractions;

/// <summary>
/// Interface for calculating a verdict based on the input.
/// </summary>
/// <typeparam name="TInput">The type of the input.</typeparam>
public interface IVerdictCalculator<in TInput>
    where TInput : notnull
{
    /// <summary>
    /// Calculates a verdict based on the provided input.
    /// </summary>
    /// <param name="input">The input to calculate the verdict from.</param>
    /// <returns>The calculated verdict.</returns>
    public Verdict Calculate(TInput input);
}
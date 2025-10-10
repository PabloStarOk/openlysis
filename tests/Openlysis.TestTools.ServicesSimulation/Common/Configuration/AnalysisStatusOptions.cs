using Microsoft.Extensions.Options;

using Openlysis.Domain.Common.Enums;

namespace Openlysis.TestTools.ServicesSimulation.Common.Configuration;

/// <summary>
/// Represents options for simulating <see cref="AnalysisStatus"/> values.
/// </summary>
/// <remarks>
/// This class provides validation rules for different simulation modes when working with <see cref="AnalysisStatus"/> values.
/// </remarks>
internal sealed record AnalysisStatusOptions : SimulatedValueOptions<AnalysisStatus>
{
    /// <inheritdoc/>
    protected override string TypeName => nameof(AnalysisStatusOptions);

    /// <inheritdoc/>
    protected override ValidateOptionsResult ValidateAsRandom()
    {
        return ValidateOptionsResult.Success;
    }

    /// <inheritdoc/>
    protected override ValidateOptionsResult ValidateAsFixed()
    {
        return FixedValue is AnalysisStatus.Queued or AnalysisStatus.InProgress
            ? FormattedFail("Fixed value cannot be Queued or InProgress.")
            : ValidateOptionsResult.Success;
    }

    /// <inheritdoc/>
    protected override ValidateOptionsResult ValidateAsRange()
    {
        return FormattedFail("Range of values is not valid for this type.");
    }

    /// <inheritdoc/>
    protected override ValidateOptionsResult ValidateAsSet()
    {
        if (ValuesSet.Count < 2)
        {
            return FormattedFail("Set must contain two or more values.");
        }

        return ValuesSet.Contains(AnalysisStatus.Queued)
            || ValuesSet.Contains(AnalysisStatus.InProgress)
            ? FormattedFail("Set cannot contain Queued or InProgress status values.")
            : ValidateOptionsResult.Success;
    }
}
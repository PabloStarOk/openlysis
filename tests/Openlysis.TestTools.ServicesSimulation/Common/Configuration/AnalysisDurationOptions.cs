using Microsoft.Extensions.Options;

namespace Openlysis.TestTools.ServicesSimulation.Common.Configuration;

/// <summary>
/// Represents configuration options for simulating duration of analysis in tests.
/// </summary>
/// <remarks>
/// This class provides configuration for different types of duration simulation strategies,
/// including fixed durations, random durations, ranges, and predefined sets of values.
/// Each strategy requires specific properties to be configured.
/// </remarks>
internal sealed record AnalysisDurationOptions : SimulatedValueOptions<int>
{
    /// <inheritdoc/>
    protected override string TypeName => nameof(AnalysisDurationOptions);

    /// <inheritdoc/>
    protected override ValidateOptionsResult ValidateAsRandom()
    {
        return FormattedFail("Random is not valid, this is to prevent mistakes.");
    }

    /// <inheritdoc/>
    protected override ValidateOptionsResult ValidateAsFixed()
    {
        return ValidateOptionsResult.Success;
    }

    /// <inheritdoc/>
    protected override ValidateOptionsResult ValidateAsRange()
    {
        if (ValuesRange is null)
        {
            return FormattedFail("Range of values must be not null.");
        }

        if (ValuesRange.Minimum < 0)
        {
            return FormattedFail("Minimum value of range must be non-negative.");
        }

        if (ValuesRange.Maximum < 0)
        {
            return FormattedFail("Maximum value of range must be non-negative.");
        }

        if (ValuesRange.Minimum >= ValuesRange.Maximum)
        {
            return FormattedFail("Minimum value of range must be less to maximum value.");
        }

        int rangeDifference = ValuesRange.Maximum - ValuesRange.Minimum;
        return rangeDifference > 1
            ? ValidateOptionsResult.Success
            : FormattedFail("Difference between range values must be greater than 2.");
    }

    /// <inheritdoc/>
    protected override ValidateOptionsResult ValidateAsSet()
    {
        return ValuesSet.Count > 1
            ? ValidateOptionsResult.Success
            : FormattedFail("Set must contain two or more values.");
    }
}
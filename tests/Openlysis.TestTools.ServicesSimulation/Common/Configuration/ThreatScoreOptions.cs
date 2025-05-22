using Microsoft.Extensions.Options;

namespace Openlysis.TestTools.ServicesSimulation.Common.Configuration;

/// <summary>
/// Represents configuration options for simulated threat score values.
/// Provides validation logic specific to threat scores, which must be between 0 and 1 inclusive.
/// </summary>
internal sealed record ThreatScoreOptions : SimulatedValueOptions<float>
{
    /// <inheritdoc/>
    protected override string TypeName => nameof(ThreatScoreOptions);

    private readonly Predicate<float> _isValueInRange = v => v is >= 0f and <= 1f;

    /// <inheritdoc/>
    protected override ValidateOptionsResult ValidateAsRandom()
    {
        return ValidateOptionsResult.Success;
    }

    /// <inheritdoc/>
    protected override ValidateOptionsResult ValidateAsFixed()
    {
        return _isValueInRange(FixedValue)
            ? ValidateOptionsResult.Success
            : FormattedFail("Fixed value must be greater or equal to 0 or lower or equal to 1.");
    }

    /// <inheritdoc/>
    protected override ValidateOptionsResult ValidateAsRange()
    {
        if (ValuesRange is null)
        {
            return FormattedFail("Range of values cannot be null.");
        }

        if (!_isValueInRange(ValuesRange.Minimum))
        {
            return FormattedFail("Minimum value of range must be greater or equal to 0 or lower or equal to 0.99.");
        }

        if (!_isValueInRange(ValuesRange.Maximum))
        {
            return FormattedFail("Maximum value of range must be greater or equal to 0 or lower or equal to 1.");
        }

        return ValuesRange.Minimum < ValuesRange.Maximum
            ? ValidateOptionsResult.Success
            : FormattedFail("Minimum value of range must be less than maximum value.");
    }

    /// <inheritdoc/>
    protected override ValidateOptionsResult ValidateAsSet()
    {
        if (ValuesSet.Count < 2)
        {
            return FormattedFail("Set must contain two or more values.");
        }

        return !ValuesSet.All(v => _isValueInRange(v))
            ? FormattedFail("All values in the set must be between 0 and 1 inclusive.")
            : ValidateOptionsResult.Success;
    }
}
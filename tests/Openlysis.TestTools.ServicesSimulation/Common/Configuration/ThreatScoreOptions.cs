using Microsoft.Extensions.Options;

namespace Openlysis.TestTools.ServicesSimulation.Common.Configuration;

/// <summary>
/// Represents configuration options for simulated threat score values.
/// Provides validation logic specific to threat scores, which must be between 0 and 1 inclusive.
/// </summary>
internal sealed record ThreatScoreOptions : SimulatedValueOptions<float>
{
    /// <summary>
    /// Gets the maximum possible value for the threat score. Must be set and is required for validation.
    /// </summary>
    public float MaxPossibleValue { get; init; } = -1;

    /// <inheritdoc/>
    protected override string TypeName => nameof(ThreatScoreOptions);

    private readonly Predicate<float> _isValidMaxPossibleValue =
        m => m is >= 0.1f and <= float.MaxValue;

    private readonly Func<float, float, bool> _isValueInRange =
        (value, max) => value >= 0f && value <= max;

    /// <inheritdoc/>
    protected override ValidateOptionsResult ValidateAsRandom()
    {
        return ValidateOptionsResult.Success;
    }

    /// <inheritdoc/>
    protected override ValidateOptionsResult ValidateAsFixed()
    {
        if (!_isValidMaxPossibleValue(MaxPossibleValue))
        {
            return FormattedFail($"The maximum possible value must be greater than or equal to 0.1 and less than or equal to {float.MaxValue}.");
        }

        return _isValueInRange(FixedValue, MaxPossibleValue)
            ? ValidateOptionsResult.Success
            : FormattedFail("The fixed value must be greater than or equal to 0 and less than or equal to the maximum possible value.");
    }

    /// <inheritdoc/>
    protected override ValidateOptionsResult ValidateAsRange()
    {
        if (ValuesRange is null)
        {
            return FormattedFail("The range of values cannot be null.");
        }

        if (!_isValidMaxPossibleValue(MaxPossibleValue))
        {
            return FormattedFail($"The maximum possible value must be greater than or equal to 0.1 and less than or equal to {float.MaxValue}.");
        }

        if (!_isValueInRange(ValuesRange.Minimum, MaxPossibleValue))
        {
            return FormattedFail("The minimum value of the range must be greater than or equal to 0 and less than the maximum possible value.");
        }

        if (!_isValueInRange(ValuesRange.Maximum, MaxPossibleValue))
        {
            return FormattedFail("The maximum value of the range must be greater than or equal to 0 and less than or equal to the maximum possible value.");
        }

        return ValuesRange.Minimum < ValuesRange.Maximum
            ? ValidateOptionsResult.Success
            : FormattedFail("The minimum value of the range must be less than the maximum value.");
    }

    /// <inheritdoc/>
    protected override ValidateOptionsResult ValidateAsSet()
    {
        if (_isValidMaxPossibleValue(MaxPossibleValue))
        {
            return FormattedFail($"Max possible value must be greater or equal to 0.1 and less than or equal to {float.MaxValue}.");
        }

        if (ValuesSet.Count < 2)
        {
            return FormattedFail("Set must contain two or more values.");
        }

        return !ValuesSet.All(v => _isValueInRange(v, MaxPossibleValue))
            ? FormattedFail($"All values in the set must be between 0 and {nameof(MaxPossibleValue)} inclusive.")
            : ValidateOptionsResult.Success;
    }
}
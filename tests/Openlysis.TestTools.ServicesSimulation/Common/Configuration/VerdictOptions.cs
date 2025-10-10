using Microsoft.Extensions.Options;

using Openlysis.Domain.Common.Enums;

namespace Openlysis.TestTools.ServicesSimulation.Common.Configuration;

/// <summary>
/// Represents configuration options for simulated <see cref="Verdict"/> values.
/// Provides validation logic specific to the <see cref="Verdict"/> enum type.
/// </summary>
internal sealed record VerdictOptions : SimulatedValueOptions<Verdict>
{
    /// <inheritdoc/>
    protected override string TypeName => nameof(VerdictOptions);

    /// <inheritdoc/>
    protected override ValidateOptionsResult ValidateAsRandom()
    {
        return ValidateOptionsResult.Success;
    }

    /// <inheritdoc/>
    protected override ValidateOptionsResult ValidateAsFixed()
    {
        return ValidateOptionsResult.Success;
    }

    /// <inheritdoc/>
    protected override ValidateOptionsResult ValidateAsRange()
    {
        return FormattedFail("Range of values is not valid for this type.");
    }

    /// <inheritdoc/>
    protected override ValidateOptionsResult ValidateAsSet()
    {
        return ValuesSet.Count > 1
            ? ValidateOptionsResult.Success
            : FormattedFail("Set must contain two or more values.");
    }
}
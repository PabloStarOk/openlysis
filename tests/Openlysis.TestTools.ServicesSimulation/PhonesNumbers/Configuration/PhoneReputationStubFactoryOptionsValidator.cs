using Microsoft.Extensions.Options;

namespace Openlysis.TestTools.ServicesSimulation.PhonesNumbers.Configuration;

/// <summary>
/// Validates the <see cref="PhoneReputationStubFactoryOptions"/> configuration options.
/// </summary>
internal sealed class PhoneReputationStubFactoryOptionsValidator
    : IValidateOptions<PhoneReputationStubFactoryOptions>
{
    /// <inheritdoc/>
    public ValidateOptionsResult Validate(string? name, PhoneReputationStubFactoryOptions options)
    {
        return options.VerdictSimulation.Validate();
    }
}
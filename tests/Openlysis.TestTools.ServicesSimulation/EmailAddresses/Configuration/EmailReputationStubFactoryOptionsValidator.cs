using Microsoft.Extensions.Options;

namespace Openlysis.TestTools.ServicesSimulation.EmailAddresses.Configuration;

/// <summary>
/// Validates the configuration options for the email reputation stub factory.
/// </summary>
internal sealed class EmailReputationStubFactoryOptionsValidator
    : IValidateOptions<EmailReputationStubFactoryOptions>
{
    /// <inheritdoc/>
    public ValidateOptionsResult Validate(string? name, EmailReputationStubFactoryOptions options)
    {
        return options.VerdictSimulation.Validate();
    }
}
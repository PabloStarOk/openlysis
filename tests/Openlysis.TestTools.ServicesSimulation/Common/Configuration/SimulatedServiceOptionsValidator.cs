using Microsoft.Extensions.Options;

using Openlysis.TestTools.ServicesSimulation.Common.Extensions;

namespace Openlysis.TestTools.ServicesSimulation.Common.Configuration;

/// <summary>
/// Validates the configuration options for a simulated service.
/// </summary>
/// <typeparam name="TStubFactoryOptions">The type of stub factory options to validate, must inherit from <see cref="StubFactoryOptions"/>.</typeparam>
internal abstract class SimulatedServiceOptionsValidator<TStubFactoryOptions>
    where TStubFactoryOptions : StubFactoryOptions
{
    /// <summary>
    /// Validates the provided simulated service options.
    /// </summary>
    /// <param name="options">The options to validate.</param>
    /// <returns>A <see cref="ValidateOptionsResult"/> indicating whether validation was successful or failed with error messages.</returns>
    protected static ValidateOptionsResult ValidateBaseOptions(
        SimulatedServiceOptions<TStubFactoryOptions> options)
    {
        ValidateOptionsResult nameResult = ValidateOptionsResult.Success;
        if (string.IsNullOrWhiteSpace(options.Name))
        {
            nameResult = ValidateOptionsResult.Fail("Name of cannot be whitespaces.");
        }

        ValidateOptionsResult verdictResult =
            options.StubFactory.VerdictSimulation.Validate();

        if (nameResult.Succeeded && verdictResult.Succeeded)
        {
            return ValidateOptionsResult.Success;
        }

        var failures = nameResult.UnionFailureMessages(verdictResult);
        return ValidateOptionsResult.Fail(failures);
   }
}
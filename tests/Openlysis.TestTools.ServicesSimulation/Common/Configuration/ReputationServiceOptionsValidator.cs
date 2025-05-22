using Microsoft.Extensions.Options;

using Openlysis.TestTools.ServicesSimulation.Common.Extensions;

namespace Openlysis.TestTools.ServicesSimulation.Common.Configuration;

/// <summary>
/// Validates configuration options for the reputation service.
/// </summary>
/// <typeparam name="TStubFactoryOptions">The type of stub factory options which must inherit from <see cref="StubFactoryOptions"/>.</typeparam>
internal sealed class ReputationServiceOptionsValidator<TStubFactoryOptions>
    : SimulatedServiceOptionsValidator<TStubFactoryOptions>,
        IValidateOptions<ReputationServiceOptions<TStubFactoryOptions>>
    where TStubFactoryOptions : StubFactoryOptions
{
    private readonly IValidateOptions<TStubFactoryOptions> _factoryValidator;

    /// <summary>
    /// Initializes a new instance of the <see cref="ReputationServiceOptionsValidator{TStubFactoryOptions}"/> class.
    /// </summary>
    /// <param name="factoryValidator">The validator for the stub factory options.</param>
    public ReputationServiceOptionsValidator(
        IValidateOptions<TStubFactoryOptions> factoryValidator)
    {
        _factoryValidator = factoryValidator;
    }

    /// <inheritdoc/>
    public ValidateOptionsResult Validate(
        string? name,
        ReputationServiceOptions<TStubFactoryOptions> options)
    {
        ValidateOptionsResult baseResult = ValidateBaseOptions(options);
        ValidateOptionsResult factoryResult = _factoryValidator.Validate(name, options.StubFactory);

        if (baseResult.Succeeded && factoryResult.Succeeded)
        {
            return ValidateOptionsResult.Success;
        }

        var failures = baseResult.UnionFailureMessages(factoryResult);
        return ValidateOptionsResult.Fail(failures);
    }
}
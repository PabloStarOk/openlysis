using Microsoft.Extensions.Options;

using Openlysis.TestTools.ServicesSimulation.Common.Extensions;

namespace Openlysis.TestTools.ServicesSimulation.Common.Configuration;

/// <summary>
/// Validates the options for an analysis service.
/// </summary>
/// <typeparam name="TStubFactoryOptions">The type of options for the stub factory, which must derive from <see cref="AnalysisStubFactoryOptions"/>.</typeparam>
internal class AnalysisServiceOptionsValidator<TStubFactoryOptions>
    : SimulatedServiceOptionsValidator<TStubFactoryOptions>,
        IValidateOptions<AnalysisServiceOptions<TStubFactoryOptions>>
    where TStubFactoryOptions : AnalysisStubFactoryOptions
{
    private readonly IValidateOptions<TStubFactoryOptions> _factoryValidator;

    /// <summary>
    /// Initializes a new instance of the <see cref="AnalysisServiceOptionsValidator{TStubFactoryOptions}"/> class.
    /// </summary>
    /// <param name="factoryValidator">The validator for stub factory options.</param>
    public AnalysisServiceOptionsValidator(
        IValidateOptions<TStubFactoryOptions> factoryValidator)
    {
        _factoryValidator = factoryValidator;
    }

    /// <inheritdoc/>
    public ValidateOptionsResult Validate(
        string? name,
        AnalysisServiceOptions<TStubFactoryOptions> options)
    {
        ValidateOptionsResult baseResult = ValidateBaseOptions(options);
        ValidateOptionsResult factoryResult = _factoryValidator.Validate(name, options.StubFactory);
        ValidateOptionsResult analysisDuration
            = options.AnalysisSecondsDuration.Validate();

        if (baseResult.Succeeded && factoryResult.Succeeded
            && analysisDuration.Succeeded)
        {
            return ValidateOptionsResult.Success;
        }

        var failures = baseResult.UnionFailureMessages(factoryResult, analysisDuration);
        return ValidateOptionsResult.Fail(failures);
    }
}
using Openlysis.Infrastructure.Shared.Configuration;

namespace Openlysis.Evaluators.Shared.Configuration;

/// <summary>
/// Defines base options required to configure a reputation evaluator.
/// </summary>
/// <remarks>
/// It must be implemented by concrete evaluator services to know the name of the configuration section.
/// </remarks>
public abstract record ReputationEvaluatorOptions : ServiceOptions;
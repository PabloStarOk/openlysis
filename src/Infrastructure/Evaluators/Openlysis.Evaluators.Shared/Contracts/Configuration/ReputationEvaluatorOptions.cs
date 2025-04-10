using Openlysis.Infrastructure.Shared.Contracts.Common.Configuration;

namespace Openlysis.Evaluators.Shared.Contracts.Configuration;

/// <summary>
/// Defines base options required to configure a reputation evaluator.
/// </summary>
/// <remarks>
/// It must be implemented by concrete evaluator services to know the name of the configuration section.
/// </remarks>
public abstract record ReputationEvaluatorOptions : ServiceOptions;
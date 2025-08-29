using Openlysis.Domain.Common.ValueObjects;

namespace Openlysis.Infrastructure.Communication.Sagas.Messages;

/// <summary>
/// Message indicating that message analysis has been completely initialized.
/// </summary>
/// <param name="CorrelationId">The correlation identifier for the analysis process.</param>
internal sealed record MessageAnalysisInitialized(GlobalId CorrelationId);
using Openlysis.Domain.Common.ValueObjects;

namespace Openlysis.Infrastructure.Shared.Communication.Contracts;

/// <summary>
/// A message to be sent to a queue, optionally containing a correlation identifier.
/// </summary>
public record QueueMessage(GlobalId? CorrelationId);
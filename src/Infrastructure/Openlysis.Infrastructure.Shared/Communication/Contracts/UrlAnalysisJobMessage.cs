using Openlysis.Domain.Common.ValueObjects;
using Openlysis.Domain.URLs.Entities;

namespace Openlysis.Infrastructure.Shared.Communication.Contracts;

/// <summary>
/// Represents a message to request a URL analysis job within a multi-analysis context.
/// </summary>
/// <param name="MultiAnalysisId">The identifier of the multi-analysis which <see cref="UrlAnalysis"/> belong to.</param>
/// <param name="Url">The URL to be analyzed.</param>
/// <param name="CorrelationId">Optional correlation identifier that associates the request to a message analysis.</param>
public record UrlAnalysisJobMessage(
    GlobalId MultiAnalysisId,
    Uri Url,
    GlobalId? CorrelationId = null)
    : AnalysisJobMessage(MultiAnalysisId, CorrelationId);
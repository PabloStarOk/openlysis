using Openlysis.Domain.Common.ValueObjects;
using Openlysis.Domain.URLs.Entities;

namespace Openlysis.Infrastructure.Shared.Communication.Contracts;

/// <summary>
/// Represents a request to analyze a URL within a multi-analysis context.
/// </summary>
/// <param name="MultiAnalysisId">The identifier of the multi-analysis which <see cref="UrlAnalysis"/> belong to.</param>
/// <param name="Url">The URL to be analyzed.</param>
public record AnalyzeUrl(
    GlobalId MultiAnalysisId,
    Uri Url);
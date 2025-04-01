using System;

using Openlysis.Domain.Common.MultiAnalyses.ValueObjects;
using Openlysis.Domain.URLs.Entities;

namespace Openlysis.MultiAnalyzer.Features.URLs.Analyze.Contracts;

/// <summary>
/// Represents a request to analyze a URL within a multi-analysis context.
/// </summary>
/// <param name="MultiAnalysisId">The identifier of the multi-analysis which <see cref="UrlServiceAnalysis"/> belong to.</param>
/// <param name="Url">The URL to be analyzed.</param>
public record AnalyzeUrl(
    MultiAnalysisId MultiAnalysisId,
    Uri Url);
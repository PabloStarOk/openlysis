using Openlysis.Domain.Common.ValueObjects;
using Openlysis.Domain.URLs;
using Openlysis.Domain.URLs.Entities;

namespace Openlysis.Infrastructure.Shared.Messaging.Models;

/// <summary>
/// Represents a request to update a <see cref="UrlMultiAnalysis"/> with one or several <see cref="UrlServiceAnalysis"/>.
/// </summary>
/// <param name="MultiAnalysisId">The ID of the multi-analysis.</param>
/// <param name="Analyses">The analyses to be updated.</param>
public record UpdateUrlMultiAnalysis(
    GlobalId MultiAnalysisId,
    params UrlServiceAnalysis[] Analyses);
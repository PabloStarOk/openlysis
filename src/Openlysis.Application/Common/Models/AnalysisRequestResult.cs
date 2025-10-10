using Openlysis.Domain.Common.Abstractions;
using Openlysis.Domain.Common.ValueObjects;

namespace Openlysis.Application.Common.Models;

/// <summary>
/// Represents the result of an analysis request, including its status and the analysis data.
/// </summary>
/// <typeparam name="TAnalysis">
/// The type of the analysis, which must be an <see cref="AggregateRoot{GlobalId}"/>.
/// </typeparam>
/// <param name="RequestStatus">The status of the analysis request.</param>
/// <param name="Analysis">The analysis data.</param>
public sealed record AnalysisRequestResult<TAnalysis>(
    AnalysisRequestStatus RequestStatus,
    TAnalysis Analysis)
    where TAnalysis : AggregateRoot<GlobalId>;
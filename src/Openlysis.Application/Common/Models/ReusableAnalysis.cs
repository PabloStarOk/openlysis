using System.Diagnostics.CodeAnalysis;

using Openlysis.Domain.Common.Abstractions;
using Openlysis.Domain.Common.ValueObjects;

namespace Openlysis.Application.Common.Models;

/// <summary>
/// Represents a reusable analysis result.
/// </summary>
/// <typeparam name="TAnalysis">The type of the analysis, which must inherit from <see cref="Entity{GlobalId}"/>.</typeparam>
/// <param name="IsReusable">Indicates whether the analysis is reusable.</param>
/// <param name="Analysis">The analysis instance if reusable; otherwise, null.</param>
public record ReusableAnalysis<TAnalysis>(
    [property: MemberNotNullWhen(true, nameof(ReusableAnalysis<TAnalysis>.Analysis))]
    bool IsReusable,
    TAnalysis? Analysis)
    where TAnalysis : Entity<GlobalId>;
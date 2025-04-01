using Openlysis.Domain.Common.Enums;

namespace Openlysis.Domain.Common.ServiceAnalyses.Mappings;

/// <summary>
/// Provides a mapping between verdicts and threat zones.
/// </summary>
public static class ThreatZoneMapping
{
    /// <summary>
    /// Gets the mapping between verdicts and threat zones.
    /// </summary>
    public static IReadOnlyDictionary<Verdict, ThreatZone> Map { get; } = new Dictionary<Verdict, ThreatZone>()
    {
        { Verdict.Unknown, ThreatZone.Unknown },
        { Verdict.Undetected, ThreatZone.Green },
        { Verdict.Suspicious, ThreatZone.Yellow },
        { Verdict.Malicious, ThreatZone.Red },
    };
}
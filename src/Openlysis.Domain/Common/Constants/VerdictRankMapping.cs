using Openlysis.Domain.Common.Enums;

namespace Openlysis.Domain.Common.Constants;

/// <summary>
/// Provides a mapping between verdicts and their corresponding severity rank values.
/// </summary>
public static class VerdictRankMapping
{
    /// <summary>
    /// A dictionary mapping verdicts to their corresponding rank values.
    /// </summary>
    public static readonly Dictionary<Verdict, int> Map = new ()
    {
        [Verdict.Malicious] = 3,
        [Verdict.Suspicious] = 2,
        [Verdict.Undetected] = 1,
        [Verdict.Unknown] = 0,
    };
}
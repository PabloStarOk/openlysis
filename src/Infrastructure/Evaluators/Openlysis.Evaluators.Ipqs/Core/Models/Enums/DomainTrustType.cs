namespace Openlysis.Evaluators.Ipqs.Core.Models.Enums;

/// <summary>
/// Represents the trust level of a domain.
/// </summary>
internal enum DomainTrustType
{
    /// <summary>
    /// The domain has not been rated.
    /// </summary>
    NotRated,

    /// <summary>
    /// The domain has a neutral trust level.
    /// </summary>
    Neutral,

    /// <summary>
    /// The domain has a positive trust level.
    /// </summary>
    Positive,

    /// <summary>
    /// The domain is considered trusted.
    /// </summary>
    Trusted,

    /// <summary>
    /// The domain is considered suspicious.
    /// </summary>
    Suspicious,

    /// <summary>
    /// The domain is considered malicious.
    /// </summary>
    Malicious,
}
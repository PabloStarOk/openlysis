using System.Text.Json.Serialization;

using Openlysis.Evaluators.Ipqs.Core.Models.Enums;

namespace Openlysis.Evaluators.Ipqs.Core.Models.Responses;

/// <summary>
/// Response returned by IPQualityScore service when verifying an email address.
/// </summary>
/// <param name="Success">Indicates whether the request was successful.</param>
/// <param name="Message">A message providing additional information about the verification.</param>
/// <param name="Errors">An array of error messages, if any occurred during verification.</param>
/// <param name="IsValid">Indicates whether the email address is valid.</param>
/// <param name="IsTimedOut">Indicates whether the verification process timed out.</param>
/// <param name="IsDisposable">Indicates whether the email address is disposable.</param>
/// <param name="IsRiskyTld">Indicates whether the email address has a risky top-level domain (TLD).</param>
/// <param name="HasProperSpfDnsRecord">Indicates whether the email domain has a proper SPF DNS record.</param>
/// <param name="HasProperDmarcDnsRecord">Indicates whether the email domain has a proper DMARC DNS record.</param>
/// <param name="OverallScore">The overall score assigned to the email address.</param>
/// <param name="FraudScore">The fraud score assigned to the email address.</param>
internal record VerifyEmailAddressResponse(
    bool Success,
    string Message,
    string[]? Errors,
    [property: JsonPropertyName("timed_out")] bool IsTimedOut,
    [property: JsonPropertyName("valid")] bool IsValid,
    [property: JsonPropertyName("disposable")] bool IsDisposable,
    [property: JsonPropertyName("risky_tld")] bool IsRiskyTld,
    [property: JsonPropertyName("spf_record")] bool HasProperSpfDnsRecord,
    [property: JsonPropertyName("dmarc_record")] bool HasProperDmarcDnsRecord,
    int OverallScore,
    float FraudScore,
    [property: JsonPropertyName("domain_trust")] string DomainTrustString)
{
    /// <summary>
    /// Gets the domain trust type by parsing the `DomainTrustString`.
    /// </summary>
    /// <remarks>
    /// The `DomainTrustString` is trimmed and spaces are removed before parsing it into a `DomainTrustType`.
    /// The parsing is case-insensitive.
    /// </remarks>
    [JsonIgnore]
    public DomainTrustType DomainTrust
    {
        get
        {
            return Enum.TryParse(
                DomainTrustString.Trim().Replace(" ", string.Empty),
                true,
                out DomainTrustType value)
                ? value
                : DomainTrustType.NotRated;
        }
    }
}
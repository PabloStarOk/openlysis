using System.Text.Json.Serialization;

namespace Openlysis.Evaluators.Ipqs.Core.Models;

/// <summary>
/// Response returned by IPQualityScore service when validating a phone number.
/// </summary>
/// <param name="Message">The message returned from the validation.</param>
/// <param name="Success">Indicates if the validation was successful.</param>
/// <param name="Errors">A list of errors encountered during validation.</param>
/// <param name="IsValid">Indicates if the phone number is properly formatted and considered valid based on assigned phone numbers available to carriers in that country.</param>
/// <param name="IsActive">Indicates if the phone number is active.</param>
/// <param name="FraudScore">The fraud score associated with the phone number.</param>
/// <param name="RecentAbuse">Indicates if the phone number has been recently abused.</param>
/// <param name="IsVoip">Indicates if the phone number is a VoIP number.</param>
/// <param name="IsRisky">Indicates if the phone number is considered risky.</param>
/// <param name="IsSpammer">Indicates if the phone number is associated with spam.</param>
/// <param name="CountryCode">The country code of the phone number.</param>
/// <param name="DialingCode">The dialing code of the phone number.</param>
/// <param name="LocalFormat">The local format of the phone number.</param>
/// <param name="LineType">The line type of the phone number.</param>
public record ValidatePhoneResponse(
    string Message,
    bool Success,
    string[]? Errors,
    [property: JsonPropertyName("valid")] bool IsValid,
    [property: JsonPropertyName("active")] bool? IsActive,
    int FraudScore,
    bool? RecentAbuse,
    [property: JsonPropertyName("VOIP")] bool? IsVoip,
    [property: JsonPropertyName("risky")] bool? IsRisky,
    [property: JsonPropertyName("spammer")] bool IsSpammer,
    [property: JsonPropertyName("country")] string CountryCode,
    int? DialingCode,
    string LocalFormat,
    string LineType);
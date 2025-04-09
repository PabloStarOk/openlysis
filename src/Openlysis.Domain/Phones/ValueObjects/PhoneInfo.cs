namespace Openlysis.Domain.Phones.ValueObjects;

/// <summary>
/// Represents phone information with local format, country code, dialing code, and line type.
/// </summary>
/// <param name="LocalFormat">The local format of the phone number.</param>
/// <param name="CountryCode">The country code of the phone number.</param>
/// <param name="DialingCode">The dialing code of the phone number.</param>
/// <param name="LineType">The type of the phone line (e.g., mobile, landline).</param>
public record PhoneInfo(
    string LocalFormat,
    string CountryCode,
    int DialingCode,
    string LineType);
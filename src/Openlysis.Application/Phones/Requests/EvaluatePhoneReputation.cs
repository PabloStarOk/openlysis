using System.ComponentModel.DataAnnotations;

namespace Openlysis.Application.Phones.Requests;

/// <summary>
/// Represents a request to evaluate the reputation of a phone number.
/// </summary>
/// <param name="Value">The phone number to evaluate.</param>
public record EvaluatePhoneReputation(string Value)
    : EvaluateReputationRequest(Value)
{
    /// <summary>
    /// Validates the format of the phone number using the <see cref="PhoneAttribute"/>.
    /// </summary>
    /// <returns>
    /// <c>true</c> if the phone number format is valid; otherwise, <c>false</c>.
    /// </returns>
    public override bool IsFormatValid()
    {
        return Validator.TryValidateValue(
            Value,
            new ValidationContext(Value),
            [],
            [new PhoneAttribute()]);
    }
}
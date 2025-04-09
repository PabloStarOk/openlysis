using System.ComponentModel.DataAnnotations;

using Openlysis.Application.Common.Requests;

namespace Openlysis.Application.Phones.Requests;

/// <summary>
/// Represents a phone number to be assessed by a service.
/// </summary>
/// <param name="Value">The phone number.</param>
public record AssessPhoneNumber(string Value)
    : AssessData(Value)
{
    /// <inheritdoc/>
    public override bool IsFormatValid()
    {
        return Validator.TryValidateValue(
            Value,
            new ValidationContext(Value),
            [],
            [new PhoneAttribute()]);
    }
}
using System.ComponentModel.DataAnnotations;

using Openlysis.Assessors.Shared.Models.Common;

namespace Openlysis.Assessors.Shared.Models.Phones;

/// <summary>
/// Represents a phone number to be assessed a service.
/// </summary>
/// <param name="Value">The phone number value.</param>
public record AssessedPhoneNumber(string Value)
    : AssessedData(Value)
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
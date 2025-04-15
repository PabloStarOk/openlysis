using System.ComponentModel.DataAnnotations;

using Openlysis.Application.Common.Requests;

namespace Openlysis.Application.EmailAddresses.Contracts.Requests;

/// <summary>
/// Represents a request to evaluate the reputation of an email address.
/// </summary>
/// <param name="Value">The email address to evaluate.</param>
public record EvaluateEmailAddressReputation(string Value)
    : EvaluateReputationRequest(Value)
{
    /// <summary>
    /// Validates the format of the email address value.
    /// </summary>
    /// <returns>
    /// True if the email address format is valid; otherwise, false.
    /// </returns>
    public override bool IsFormatValid()
    {
        return Validator.TryValidateValue(
            Value,
            new ValidationContext(Value),
            [],
            [new EmailAddressAttribute()]);
    }
}
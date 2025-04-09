using System.Text.Json.Serialization;

using FastEndpoints;

using Microsoft.AspNetCore.Mvc;

namespace Openlysis.API.Endpoints.Phones.GetReputation;

/// <summary>
/// Represents a request to retrieve the reputation of a phone number.
/// </summary>
/// <param name="PhoneNumber">The phone number to check in E.164 format.</param>
public record GetReputationRequest(
    [property: FromRoute,
               BindFrom("phone-number")]
    string PhoneNumber)
{
    /// <summary>
    /// Gets the normalized phone number.
    /// </summary>
    /// <remarks>
    /// This property performs the following operations to normalize the phone number:
    /// - Trims leading and trailing whitespace.
    /// - Removes all spaces within the phone number.
    /// </remarks>
    /// <value>A normalized phone number as a string.</value>
    [JsonIgnore]
    public string NormalizedPhoneNumber => PhoneNumber
        .Trim()
        .Replace(" ", string.Empty);
}
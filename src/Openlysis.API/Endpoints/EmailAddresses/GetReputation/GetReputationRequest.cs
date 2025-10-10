using System.Text.Json.Serialization;

using FastEndpoints;

using Microsoft.AspNetCore.Mvc;

namespace Openlysis.API.Endpoints.EmailAddresses.GetReputation;

/// <summary>
/// Represents a request to get the reputation of an email address.
/// </summary>
/// <param name="EmailAddress">
/// The email address for which the reputation is being requested.
/// This value is bound from the route parameter `email-address`.
/// </param>
public record GetReputationRequest(
    [property: BindFrom("email-address"), FromRoute]
    string EmailAddress)
{
    /// <summary>
    /// Gets the normalized email address.
    /// </summary>
    /// <remarks>
    /// This property performs the following operations to normalize the email address:
    /// - Trims leading and trailing whitespace.
    /// - Removes all spaces within the email address.
    /// </remarks>
    /// <value>A normalized email address as a string.</value>
    [JsonIgnore]
    public string NormalizedEmailAddress => EmailAddress
        .Trim()
        .Replace(" ", string.Empty);
}
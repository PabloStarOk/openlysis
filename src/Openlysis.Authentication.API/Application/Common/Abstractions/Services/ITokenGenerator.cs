using Openlysis.Domain.Users.Entities;

namespace Openlysis.Authentication.API.Application.Common.Abstractions.Services;

/// <summary>
/// Defines a contract for generating authentication tokens for a given user.
/// </summary>
internal interface ITokenGenerator
{
    /// <summary>
    /// Generates an access token for the specified user.
    /// </summary>
    /// <param name="user">The user for whom to generate the access token.</param>
    /// <returns>An access token as a string.</returns>
    public string GenerateAccessToken(User user);
}
using Openlysis.Authentication.API.Application.Common.Models;
using Openlysis.Domain.Users.Entities;

namespace Openlysis.Authentication.API.Application.Common.Abstractions.Services;

/// <summary>
/// Defines a contract for generating authentication tokens for a given user.
/// </summary>
internal interface ITokenGenerator
{
    /// <summary>
    /// Generates authentication tokens for the specified user.
    /// </summary>
    /// <param name="user">The user for whom to generate tokens.</param>
    /// <returns>An <see cref="AuthTokens"/> object containing the generated tokens.</returns>
    public AuthTokens Generate(User user);
}
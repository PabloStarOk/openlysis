using FastEndpoints;

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;

namespace Openlysis.API.Authentication.API.Endpoints;

/// <summary>
/// Represents a group of authentication-related endpoints.
/// </summary>
public class AuthenticationGroup : Group
{
    /// <summary>
    /// Initializes a new instance of the <see cref="AuthenticationGroup"/> class.
    /// </summary>
    public AuthenticationGroup()
    {
        Configure("auth", ep =>
        {
            ep.Description(builder =>
                {
                    builder.WithGroupName("Auth");
                    builder.WithDisplayName("Auth");
                    builder.WithTags("Auth");
                });
        });
    }
}
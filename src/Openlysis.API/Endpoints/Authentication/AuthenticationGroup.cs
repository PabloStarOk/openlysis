using FastEndpoints;

namespace Openlysis.API.Endpoints.Authentication;

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
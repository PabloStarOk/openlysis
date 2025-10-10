using FastEndpoints;

namespace Openlysis.API.Endpoints.Users;

/// <summary>
/// Represents the API group for user-related endpoints.
/// Configures the group with the route prefix "users" and sets metadata such as group name, display name, and tags.
/// </summary>
public class UsersGroup : Group
{
    /// <summary>
    /// Initializes a new instance of the <see cref="UsersGroup"/> class.
    /// Configures the group endpoint with descriptive metadata.
    /// </summary>
    public UsersGroup()
    {
        Configure("users", ep =>
        {
            ep.Description(
                b =>
                {
                    b.WithGroupName("Users");
                    b.WithDisplayName("Users");
                });
        });
    }
}
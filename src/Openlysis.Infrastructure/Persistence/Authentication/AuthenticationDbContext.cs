using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

using Openlysis.Infrastructure.Persistence.Authentication.Models;

namespace Openlysis.Infrastructure.Persistence.Authentication;

/// <summary>
/// Represents the database context for authentication, inheriting from <see cref="IdentityDbContext{IdentityUser}"/>.
/// </summary>
public class AuthenticationDbContext : IdentityDbContext<User>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="AuthenticationDbContext"/> class.
    /// </summary>
    /// <param name="options">Options to configure the <see cref="DbContext"/>.</param>
    public AuthenticationDbContext(DbContextOptions<AuthenticationDbContext> options)
    : base(options)
    {
    }
}

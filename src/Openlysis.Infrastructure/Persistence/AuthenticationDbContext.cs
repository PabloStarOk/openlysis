using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Openlysis.Infrastructure.Persistence;

/// <summary>
/// Represents the database context for authentication, inheriting from <see cref="IdentityDbContext{IdentityUser}"/>.
/// </summary>
public class AuthenticationDbContext : IdentityDbContext<IdentityUser>
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

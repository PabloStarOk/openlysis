using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

using Openlysis.API.Authentication.Infrastructure.Persistence.Models;

namespace Openlysis.API.Authentication.Infrastructure.Persistence;

/// <summary>
/// Represents the database context for authentication, inheriting from <see cref="IdentityDbContext"/>.
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

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Openlysis.Infrastructure.Persistence.Designs;

/// <summary>
/// Factory for creating instances of <see cref="AuthenticationDbContext"/> at design time.
/// </summary>
public class AuthenticationDbContextFactory : IDesignTimeDbContextFactory<AuthenticationDbContext>
{
    /// <inheritdoc/>
    public AuthenticationDbContext CreateDbContext(string[] args)
    {
        var builder = new DbContextOptionsBuilder<AuthenticationDbContext>();
        builder.UseSqlServer();
        return new AuthenticationDbContext(builder.Options);
    }
}
using System.Data;
using System.Data.Common;

namespace Openlysis.Authentication.API.Infrastructure.Persistence;

/// <summary>
/// Authentication database context for managing connections.
/// </summary>
internal sealed class AuthDbContext
{
    private readonly DbDataSource _dbDataSource;

    /// <summary>
    /// Initializes a new instance of the <see cref="AuthDbContext"/> class with the specified data source.
    /// </summary>
    /// <param name="dbDataSource">The database data source to use for connections.</param>
    public AuthDbContext(DbDataSource dbDataSource)
    {
        _dbDataSource = dbDataSource;
    }

    /// <summary>
    /// Asynchronously creates and opens a new database connection.
    /// </summary>
    /// <returns>An open <see cref="IDbConnection"/> instance.</returns>
    public async Task<DbConnection> OpenConnectionAsync()
    {
        return await _dbDataSource.OpenConnectionAsync();
    }
}
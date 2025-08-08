using System.Data;

using Npgsql;

namespace Openlysis.Authentication.API.Infrastructure.Persistence;

/// <summary>
/// Authentication database context for managing connections.
/// </summary>
internal sealed class AuthDbContext
{
    private readonly string _connectionString;

    /// <summary>
    /// Initializes a new instance of the <see cref="AuthDbContext"/> class with the specified connection string.
    /// </summary>
    /// <param name="connectionString">The connection string to the database.</param>
    public AuthDbContext(string connectionString)
    {
        _connectionString = connectionString;
    }

    /// <summary>
    /// Creates and returns a new <see cref="IDbConnection"/> to the database.
    /// </summary>
    /// <returns>An <see cref="IDbConnection"/> instance.</returns>
    public IDbConnection CreateConnection()
    {
        return new NpgsqlConnection(_connectionString);
    }
}
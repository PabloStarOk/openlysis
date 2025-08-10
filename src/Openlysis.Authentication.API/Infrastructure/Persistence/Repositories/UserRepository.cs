using Dapper;

using Openlysis.Authentication.API.Application.Common.Abstractions.Persistence;
using Openlysis.Authentication.API.Infrastructure.Persistence.Entities;
using Openlysis.Domain.Common.ValueObjects;
using Openlysis.Domain.Users.Entities;
using Openlysis.Domain.Users.ValueObjects;

namespace Openlysis.Authentication.API.Infrastructure.Persistence.Repositories;

/// <summary>
/// Repository implementation for user-related persistence operations using Dapper.
/// </summary>
[DapperAot]
internal sealed class UserRepository : IUserRepository
{
    private const string AddSqlQuery =
        """
        INSERT INTO users(user_id, email_address, password_hash, password_hash_salt)
        VALUES (@user_id, @email_address, @password_hash, @password_hash_salt)
        """;

    private const string GetByIdSqlQuery =
        """
        SELECT user_id, email_address, password_hash, password_hash_salt
        FROM users
        WHERE user_id = @id
        """;

    private const string GetByEmailSqlQuery =
        """
        SELECT user_id, email_address, password_hash, password_hash_salt
        FROM users
        WHERE email_address = @email_address
        """;

    private const string ExistsSqlQuery =
        "SELECT EXISTS (SELECT 1 FROM users WHERE email_address = @email_address);";

    private readonly AuthDbContext _dbContext;

    /// <summary>
    /// Initializes a new instance of the <see cref="UserRepository"/> class with the specified <see cref="AuthDbContext"/>.
    /// </summary>
    /// <param name="dbContext">The authentication database context.</param>
    public UserRepository(AuthDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    /// <inheritdoc/>
    public async Task AddAsync(User user)
    {
        using var dbConnection = _dbContext.CreateConnection();
        await dbConnection.ExecuteAsync(AddSqlQuery, new
        {
            user_id = user.Id.Value,
            email_address = user.Email.Value,
            password_hash = user.PasswordHash,
            password_hash_salt = user.PasswordHashSalt,
        });
    }

    /// <inheritdoc/>
    public async Task<User> GetByIdAsync(GlobalId id)
    {
        using var dbConnection = _dbContext.CreateConnection();
        var dbEntity = await dbConnection.QuerySingleAsync<DbUser>(
            GetByIdSqlQuery,
            new { id = id.Value });

        return dbEntity.ToDomainUser();
    }

    /// <inheritdoc/>
    public async Task<User> GetByEmailAsync(EmailAddress emailAddress)
    {
        using var dbConnection = _dbContext.CreateConnection();
        var dbEntity = await dbConnection.QuerySingleAsync<DbUser>(
            GetByEmailSqlQuery,
            new { email_address = emailAddress.Value });

        return dbEntity.ToDomainUser();
    }

    /// <inheritdoc/>
    public async Task<bool> ExistsAsync(EmailAddress emailAddress)
    {
        using var dbConnection = _dbContext.CreateConnection();
        return await dbConnection.QuerySingleAsync<bool>(ExistsSqlQuery, new
        {
            email_address = emailAddress.Value,
        });
    }
}
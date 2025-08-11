using Dapper;

using ErrorOr;

using Openlysis.Authentication.API.Application.Common.Abstractions.Persistence;
using Openlysis.Authentication.API.Infrastructure.Persistence.Entities;
using Openlysis.Domain.Common.ValueObjects;
using Openlysis.Domain.Users.Entities;

namespace Openlysis.Authentication.API.Infrastructure.Persistence.Repositories;

/// <summary>
/// Repository for managing refresh tokens using Dapper for data access.
/// </summary>
[DapperAot]
internal sealed class RefreshTokenRepository : IRefreshTokenRepository
{
    private const string AddSqlQuery =
        """
        INSERT INTO refresh_tokens(token_hash, created_at, expires_at, revoked_at, user_id)
        VALUES (@token_hash, @created_at, @expires_at, @revoked_at, @user_id)
        """;

    private const string GetByHashSqlQuery =
        """
        SELECT token_id, token_hash, created_at, expires_at, revoked_at, user_id
        FROM refresh_tokens
        WHERE token_hash = @token_hash
        """;

    private const string UpdateSqlQuery =
        "UPDATE refresh_tokens SET revoked_at = @revoked_at WHERE token_id = @id";

    private const string RevokeAllForUserSqlQuery =
        "UPDATE refresh_tokens SET revoked_at = now() WHERE user_id = @user_id";

    private readonly AuthDbContext _dbContext;

    /// <summary>
    /// Initializes a new instance of the <see cref="RefreshTokenRepository"/> class.
    /// </summary>
    /// <param name="dbContext">The authentication database context.</param>
    public RefreshTokenRepository(AuthDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    /// <inheritdoc/>
    public async Task AddAsync(RefreshToken token)
    {
        await using var dbConnection = await _dbContext.OpenConnectionAsync();
        await dbConnection.ExecuteAsync(AddSqlQuery, new
        {
            token_hash = token.TokenHash,
            created_at = token.CreatedAt,
            expires_at = token.ExpiresAt,
            revoked_at = token.RevokedAt,
            user_id = token.UserId.Value,
        });
    }

    /// <inheritdoc/>
    public async Task<RefreshToken?> GetByHashAsync(byte[] hash)
    {
        await using var dbConnection = await _dbContext.OpenConnectionAsync();
        var refreshToken = await dbConnection.QuerySingleOrDefaultAsync<DbRefreshToken>(
            GetByHashSqlQuery,
            new { token_hash = hash });
        return refreshToken?.ToDomainToken();
    }

    /// <inheritdoc/>
    public async Task<ErrorOr<Success>> RevokeAndAddAsync(
        RefreshToken revokedToken,
        RefreshToken newToken)
    {
        await using var dbConnection = await _dbContext.OpenConnectionAsync();
        await using var transaction = await dbConnection.BeginTransactionAsync();
        await dbConnection.ExecuteAsync(
            UpdateSqlQuery,
            new
            {
                id = revokedToken.Id,
                revoked_at = revokedToken.RevokedAt,
            },
            transaction);

        await dbConnection.ExecuteAsync(
            AddSqlQuery,
            new
            {
                token_hash = newToken.TokenHash,
                created_at = newToken.CreatedAt,
                expires_at = newToken.ExpiresAt,
                revoked_at = newToken.RevokedAt,
                user_id = newToken.UserId.Value,
            },
            transaction);

        try
        {
            await transaction.CommitAsync();
            return Result.Success;
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }

    /// <inheritdoc/>
    public async Task RevokeAllForUserAsync(GlobalId userId)
    {
        await using var dbConnection = await _dbContext.OpenConnectionAsync();
        await dbConnection.ExecuteAsync(
            RevokeAllForUserSqlQuery,
            new { user_id = userId.Value });
    }
}
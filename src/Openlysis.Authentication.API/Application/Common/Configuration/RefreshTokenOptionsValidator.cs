using Microsoft.Extensions.Options;

namespace Openlysis.Authentication.API.Application.Common.Configuration;

/// <summary>
/// Validates <see cref="RefreshTokenOptions"/> to ensure all required properties are set and valid.
/// </summary>
internal sealed class RefreshTokenOptionsValidator : IValidateOptions<RefreshTokenOptions>
{
    /// <inheritdoc/>
    public ValidateOptionsResult Validate(string? name, RefreshTokenOptions options)
    {
        var errors = new List<string>();

        if (options.ExpirationDays < 1)
        {
            errors.Add($"{nameof(options.ExpirationDays)} must be at least 1.");
        }

        return errors.Count > 0
            ? ValidateOptionsResult.Fail(string.Join(" ", errors))
            : ValidateOptionsResult.Success;
    }
}

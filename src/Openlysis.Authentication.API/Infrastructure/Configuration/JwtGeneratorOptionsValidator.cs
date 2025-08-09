using Microsoft.Extensions.Options;

namespace Openlysis.Authentication.API.Infrastructure.Configuration;

/// <summary>
/// Validates <see cref="JwtGeneratorOptions"/> to ensure all required properties are set and valid.
/// </summary>
internal sealed class JwtGeneratorOptionsValidator : IValidateOptions<JwtGeneratorOptions>
{
    /// <inheritdoc/>
    public ValidateOptionsResult Validate(string? name, JwtGeneratorOptions options)
    {
        var errors = new List<string>();

        if (options.ExpirationSeconds < 1)
        {
            errors.Add($"{nameof(options.ExpirationSeconds)} must be at least 1.");
        }

        if (string.IsNullOrWhiteSpace(options.Issuer))
        {
            errors.Add($"{nameof(options.Issuer)} is required.");
        }

        if (string.IsNullOrWhiteSpace(options.Audience))
        {
            errors.Add($"{nameof(options.Audience)} is required.");
        }

        return errors.Count > 0
            ? ValidateOptionsResult.Fail(string.Join(" ", errors))
            : ValidateOptionsResult.Success;
    }
}

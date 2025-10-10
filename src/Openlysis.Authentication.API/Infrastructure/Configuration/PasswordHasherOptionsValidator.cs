using Microsoft.Extensions.Options;

namespace Openlysis.Authentication.API.Infrastructure.Configuration;

/// <summary>
/// Validates <see cref="PasswordHasherOptions"/> to ensure all configuration values meet minimum security requirements.
/// </summary>
internal sealed class PasswordHasherOptionsValidator : IValidateOptions<PasswordHasherOptions>
{
    /// <inheritdoc/>
    public ValidateOptionsResult Validate(string? name, PasswordHasherOptions options)
    {
        var errors = new List<string>();
        if (options.HashSizeBytes < PasswordHasherOptions.DefaultHashSizeBytes)
        {
            errors.Add($"{nameof(options.HashSizeBytes)} must be at least {PasswordHasherOptions.DefaultHashSizeBytes} bytes.");
        }

        if (options.SaltSizeBytes < PasswordHasherOptions.DefaultSaltSizeBytes)
        {
            errors.Add($"{nameof(options.SaltSizeBytes)} must be at least {PasswordHasherOptions.DefaultSaltSizeBytes} bytes.");
        }

        if (options.MemorySizeKibibytes < PasswordHasherOptions.DefaultMemorySizeKibibytes)
        {
            errors.Add($"{nameof(options.MemorySizeKibibytes)} must be at least {PasswordHasherOptions.DefaultMemorySizeKibibytes} KiB.");
        }

        if (options.Iterations < PasswordHasherOptions.DefaultIterations)
        {
            errors.Add($"{nameof(options.Iterations)} must be at least {PasswordHasherOptions.DefaultIterations}.");
        }

        if (options.ParallelismDegree < PasswordHasherOptions.DefaultParallelismDegree)
        {
            errors.Add($"{nameof(options.ParallelismDegree)} must be at least {PasswordHasherOptions.DefaultParallelismDegree}.");
        }

        return errors.Count > 0
            ? ValidateOptionsResult.Fail(string.Join(" ", errors))
            : ValidateOptionsResult.Success;
    }
}

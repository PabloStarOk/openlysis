using Microsoft.Extensions.Options;

namespace Openlysis.Authentication.API.Endpoints.Configuration;

/// <summary>
/// Validates <see cref="PasswordRequirements"/> options to ensure password policy constraints are met.
/// </summary>
internal sealed class PasswordRequirementsValidator : IValidateOptions<PasswordRequirements>
{
    /// <inheritdoc/>
    public ValidateOptionsResult Validate(string? name, PasswordRequirements options)
    {
        var errors = new List<string>();

        if (options.MinLength < 1)
        {
            errors.Add($"{nameof(options.MinLength)} must be at least 1.");
        }

        if (options.MinLowerChars < 0)
        {
            errors.Add($"{nameof(options.MinLowerChars)} cannot be negative.");
        }

        if (options.MinUpperChars < 0)
        {
            errors.Add($"{nameof(options.MinUpperChars)} cannot be negative.");
        }

        if (options.MinDigits < 0)
        {
            errors.Add($"{nameof(options.MinDigits)} cannot be negative.");
        }

        if (options.MinSpecialChars < 0)
        {
            errors.Add($"{nameof(options.MinSpecialChars)} cannot be negative.");
        }

        var totalRequired = options.MinLowerChars + options.MinUpperChars + options.MinDigits + options.MinSpecialChars;
        if (totalRequired > options.MinLength)
        {
            errors.Add("Sum of minimum required character types cannot exceed minimum password length.");
        }

        return errors.Count > 0 ? ValidateOptionsResult.Fail(string.Join(" ", errors))
            : ValidateOptionsResult.Success;
    }
}
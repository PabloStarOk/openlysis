using Microsoft.Extensions.Options;

namespace Doppler.NET.Configuration;

/// <summary>
/// Validates <see cref="DopplerClientOptions"/> to ensure all required properties are set and valid.
/// </summary>
public sealed class DopplerClientOptionsValidator : IValidateOptions<DopplerClientOptions>
{
    /// <inheritdoc/>
    public ValidateOptionsResult Validate(string? name, DopplerClientOptions options)
    {
        var errors = new List<string>();

        if (string.IsNullOrWhiteSpace(options.ServiceTokenEnvVariable))
        {
            errors.Add($"{nameof(options.ServiceTokenEnvVariable)} is required.");
        }

        if (string.IsNullOrWhiteSpace(options.ProjectName))
        {
            errors.Add($"{nameof(options.ProjectName)} is required.");
        }

        if (string.IsNullOrWhiteSpace(options.ConfigName))
        {
            errors.Add($"{nameof(options.ConfigName)} is required.");
        }

        return errors.Count > 0
            ? ValidateOptionsResult.Fail(string.Join(" ", errors))
            : ValidateOptionsResult.Success;
    }
}

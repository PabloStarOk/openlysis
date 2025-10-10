using Microsoft.Extensions.Options;

namespace Openlysis.Infrastructure.Shared.Infrastructure.Secrets;

/// <summary>
/// Validates <see cref="DopplerSecretsProviderOptions"/> to ensure all required properties are set and valid.
/// </summary>
internal sealed class ApiKeyProviderOptionsValidator : IValidateOptions<DopplerSecretsProviderOptions>
{
    /// <inheritdoc/>
    public ValidateOptionsResult Validate(string? name, DopplerSecretsProviderOptions options)
    {
        var errors = new List<string>();

        if (options.ApiKeySecretNames.Length < 1)
        {
            errors.Add($"{nameof(options.ApiKeySecretNames)} is required.");
        }

        return errors.Count > 0
            ? ValidateOptionsResult.Fail(string.Join(" ", errors))
            : ValidateOptionsResult.Success;
    }
}

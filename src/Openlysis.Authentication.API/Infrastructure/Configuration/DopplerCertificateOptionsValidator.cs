using Microsoft.Extensions.Options;

namespace Openlysis.Authentication.API.Infrastructure.Configuration;

/// <summary>
/// Validates <see cref="DopplerCertificateOptions"/> to ensure all required properties are set and valid.
/// </summary>
internal sealed class DopplerCertificateOptionsValidator : IValidateOptions<DopplerCertificateOptions>
{
    /// <inheritdoc/>
    public ValidateOptionsResult Validate(string? name, DopplerCertificateOptions options)
    {
        var errors = new List<string>();

        if (string.IsNullOrWhiteSpace(options.CertificateSecretName))
        {
            errors.Add($"{nameof(options.CertificateSecretName)} is required.");
        }

        if (string.IsNullOrWhiteSpace(options.PasswordSecretName))
        {
            errors.Add($"{nameof(options.PasswordSecretName)} is required.");
        }

        return errors.Count > 0
            ? ValidateOptionsResult.Fail(string.Join(" ", errors))
            : ValidateOptionsResult.Success;
    }
}

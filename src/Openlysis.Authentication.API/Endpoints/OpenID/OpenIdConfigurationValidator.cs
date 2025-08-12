using Microsoft.Extensions.Options;

namespace Openlysis.Authentication.API.Endpoints.OpenID;

/// <summary>
/// Validates the <see cref="OpenIdConfiguration"/> options to ensure all required properties are set and valid.
/// </summary>
internal sealed class OpenIdConfigurationValidator : IValidateOptions<OpenIdConfiguration>
{
    /// <inheritdoc/>
    public ValidateOptionsResult Validate(string? name, OpenIdConfiguration options)
    {
        var errors = new List<string>();

        if (string.IsNullOrWhiteSpace(options.Issuer))
        {
            errors.Add($"{nameof(options.Issuer)} must not be null or empty.");
        }

        if (!options.TokenEndpoint.IsAbsoluteUri)
        {
            errors.Add($"{nameof(options.TokenEndpoint)} must be an absolute URI.");
        }

        if (!options.JwksUri.IsAbsoluteUri)
        {
            errors.Add($"{nameof(options.JwksUri)} must be an absolute URI.");
        }

        if (options.SubjectTypesSupported.Length is 0)
        {
            errors.Add($"{nameof(options.SubjectTypesSupported)} must not be empty.");
        }

        if (options.IdTokenSigningAlgValuesSupported.Length is 0)
        {
            errors.Add($"{nameof(options.IdTokenSigningAlgValuesSupported)} must not be empty.");
        }

        return errors.Count > 0
            ? ValidateOptionsResult.Fail(string.Join(" ", errors))
            : ValidateOptionsResult.Success;
    }
}

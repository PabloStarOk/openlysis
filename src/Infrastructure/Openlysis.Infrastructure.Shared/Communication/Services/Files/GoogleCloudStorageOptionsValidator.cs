namespace Openlysis.Infrastructure.Shared.Communication.Services.Files;

using Microsoft.Extensions.Options;

/// <summary>
/// Validates <see cref="GoogleCloudStorageOptions"/> to ensure all required properties are set and valid.
/// </summary>
internal sealed class GoogleCloudStorageOptionsValidator : IValidateOptions<GoogleCloudStorageOptions>
{
    /// <inheritdoc/>
    public ValidateOptionsResult Validate(string? name, GoogleCloudStorageOptions options)
    {
        var errors = new List<string>();

        if (string.IsNullOrWhiteSpace(options.BucketName))
        {
            errors.Add($"{nameof(options.BucketName)} is required.");
        }

        return errors.Count > 0
            ? ValidateOptionsResult.Fail(string.Join(" ", errors))
            : ValidateOptionsResult.Success;
    }
}
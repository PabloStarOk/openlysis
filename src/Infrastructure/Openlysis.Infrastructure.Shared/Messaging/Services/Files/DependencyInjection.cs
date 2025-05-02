using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

using Openlysis.Infrastructure.Shared.Messaging.Abstractions;

namespace Openlysis.Infrastructure.Shared.Messaging.Services.Files;

/// <summary>
/// Provides methods for registering dependencies related to messaging services.
/// </summary>
internal static class DependencyInjection
{
    /// <summary>
    /// Registers the local file storage provider and its dependencies into the service collection.
    /// </summary>
    /// <param name="services">The service collection to which the dependencies will be added.</param>
    /// <param name="configuration">The configuration instance used to configure the provider.</param>
    internal static void AddLocalFileStorageProvider(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // Get options
        var options = configuration.GetSection(
            LocalFileStorageProviderOptions.SectionName)
            .Get<LocalFileStorageProviderOptions>();

        DirectoryInfo dirInfo;
        if (options is not null)
        {
            ThrowIfInvalidOptions(options);
            dirInfo = new DirectoryInfo(options.StorageRootDirectoryPath);
        }
        else
        {
            string tempSubDirPath = Path.Combine(Path.GetTempPath(), "openlysis");
            dirInfo = new DirectoryInfo(tempSubDirPath);
        }

        if (!dirInfo.Exists)
        {
            dirInfo.Create();
        }

        if (OperatingSystem.IsLinux())
        {
            dirInfo.UnixFileMode =
                UnixFileMode.UserExecute
                | UnixFileMode.UserWrite
                | UnixFileMode.UserRead;
        }

        services.AddSingleton(dirInfo);
        services.AddScoped<IFileStorageProvider, LocalFileStorageProvider>();
    }

    /// <summary>
    /// Validates the provided <see cref="LocalFileStorageProviderOptions"/> instance.
    /// Throws an <see cref="InvalidOperationException"/> if the options are invalid.
    /// </summary>
    /// <param name="options">The options to validate.</param>
    /// <exception cref="InvalidOperationException">Thrown when the options are invalid.</exception>
    private static void ThrowIfInvalidOptions(LocalFileStorageProviderOptions options)
    {
        ValidateOptionsResult validationResult =
            LocalFileStorageProviderOptionsValidator.Validate(options);

        if (validationResult.Failed)
        {
            throw new InvalidOperationException(validationResult.FailureMessage);
        }
    }
}
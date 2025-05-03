using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

using Openlysis.Infrastructure.Shared.Communication.Abstractions;

namespace Openlysis.Infrastructure.Shared.Communication.Services.Files;

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
            if (!dirInfo.Exists)
            {
                dirInfo.Create();
            }
        }

        if (!HasEnoughPermissions(dirInfo)
            && !TrySetLinuxPermissions(dirInfo))
        {
            throw new InvalidOperationException("The directory does not have sufficient permissions, and an attempt to set the required permissions failed.");
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

    /// <summary>
    /// Creates a temporary file in the specified directory.
    /// </summary>
    /// <param name="directoryInfo">The directory in which the temporary file will be created.</param>
    /// <returns>The full path of the created temporary file.</returns>
    private static string CreateTemporaryFile(DirectoryInfo directoryInfo)
    {
        string tempFileName = Path.GetRandomFileName();
        string tempFilePath = Path.Combine(directoryInfo.FullName, tempFileName);
        using var fileStream = File.Create(tempFilePath);
        return tempFilePath;
    }

    /// <summary>
    /// Checks if the specified directory is writable by attempting to create a temporary file in it.
    /// </summary>
    /// <param name="directoryInfo">The directory to check for write permissions.</param>
    /// <returns><c>true</c> if the directory is writable; otherwise, <c>false</c>.</returns>
    private static bool IsDirectoryWritable(DirectoryInfo directoryInfo)
    {
        if (!directoryInfo.Exists)
        {
            throw new ArgumentException("Directory does not exist.", nameof(directoryInfo));
        }

        try
        {
            CreateTemporaryFile(directoryInfo);
        }
        catch (UnauthorizedAccessException)
        {
            return false;
        }

        return true;
    }

    /// <summary>
    /// Checks if the specified directory is readable by attempting to create and read a temporary file in it.
    /// </summary>
    /// <param name="directoryInfo">The directory to check for read permissions.</param>
    /// <returns><c>true</c> if the directory is readable; otherwise, <c>false</c>.</returns>
    private static bool IsDirectoryReadable(DirectoryInfo directoryInfo)
    {
        if (!directoryInfo.Exists)
        {
            throw new ArgumentException("Directory does not exist.", nameof(directoryInfo));
        }

        string tempFilePath = CreateTemporaryFile(directoryInfo);

        try
        {
            using var fileStream = File.OpenRead(tempFilePath);
            fileStream.ReadByte();
        }
        catch (UnauthorizedAccessException)
        {
            return false;
        }

        return true;
    }

    /// <summary>
    /// Checks if the specified directory has sufficient permissions for the application to operate.
    /// </summary>
    /// <param name="directoryInfo">The directory to check for permissions.</param>
    /// <returns><c>true</c> if the directory has sufficient permissions; otherwise, <c>false</c>.</returns>
    private static bool HasEnoughPermissions(DirectoryInfo directoryInfo)
    {
        UnixFileMode traversePermissions =
            directoryInfo.UnixFileMode & UnixFileMode.UserExecute;
        if (OperatingSystem.IsLinux()
            && traversePermissions is 0)
        {
            return false;
        }

        return IsDirectoryWritable(directoryInfo)
            && IsDirectoryReadable(directoryInfo);
    }

    /// <summary>
    /// Attempts to set the necessary permissions for the specified directory on Linux systems.
    /// </summary>
    /// <param name="directoryInfo">The directory for which permissions will be set.</param>
    /// <returns>
    /// <c>true</c> if the permissions were successfully set; otherwise, <c>false</c>.
    /// </returns>
    private static bool TrySetLinuxPermissions(DirectoryInfo directoryInfo)
    {
        if (!OperatingSystem.IsLinux())
        {
            return false;
        }

        try
        {
            directoryInfo.UnixFileMode =
                UnixFileMode.UserExecute
                | UnixFileMode.UserWrite
                | UnixFileMode.UserRead;
            return true;
        }
        catch (UnauthorizedAccessException)
        {
            return false;
        }
    }
}
using Microsoft.Extensions.DependencyInjection;

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
    internal static void AddLocalFileStorageProvider(
        this IServiceCollection services)
    {
        string tempSubDirPath = Path.Combine(Path.GetTempPath(), "openlysis");
        var dirInfo = new DirectoryInfo(tempSubDirPath);
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
}
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;

using Openlysis.Infrastructure.Shared.Infrastructure.ConfigLoader.Configuration;

namespace Openlysis.Infrastructure.Shared.Infrastructure.ConfigLoader;

/// <summary>
/// Provides extension methods and utilities for configuring application configuration
/// through directories and JSON files.
/// </summary>
/// <remarks>
/// This class contains methods to extend IConfigurationBuilder with the ability to load
/// configuration from specified directories and JSON files based on ConfigPathsOptions.
/// </remarks>
public static class DependencyInjection
{
    /// <summary>
    /// Extends IConfigurationBuilder to load configuration from specified directories and JSON files.
    /// </summary>
    /// <param name="configBuilder">The configuration builder to extend.</param>
    /// <remarks>
    /// This method reads configuration paths from the existing configuration, then adds
    /// directories and JSON files specified in the ConfigPathsOptions to the configuration builder.
    /// </remarks>
    public static void UseConfigLoader(
        this IConfigurationBuilder configBuilder)
    {
        IConfiguration currentConfig = configBuilder.Build();
        ConfigPathsOptions pathsOptions = GetConfigPathOptions(currentConfig);
        AddDirectories(configBuilder, pathsOptions.Directories);
        AddJsonFiles(configBuilder, pathsOptions.JsonFiles);
    }

    /// <summary>
    /// Gets configuration path options from the provided configuration.
    /// </summary>
    /// <param name="configuration">The configuration to extract options from.</param>
    /// <returns>A validated ConfigPathsOptions object containing directory and file paths.</returns>
    /// <exception cref="ArgumentNullException">Thrown when the options section is null.</exception>
    /// <exception cref="OptionsValidationException">Thrown when the options fail validation.</exception>
    private static ConfigPathsOptions GetConfigPathOptions(
        IConfiguration configuration)
    {
        var options = configuration
            .GetRequiredSection(ConfigPathsOptions.SectionName)
            .Get<ConfigPathsOptions>();
        ArgumentNullException.ThrowIfNull(options);

        ValidateOptionsResult validationResult =
            ConfigPathsOptionsValidator.Validate(options);
        if (validationResult.Failed)
        {
            throw new OptionsValidationException(
                string.Empty,
                typeof(ConfigPathsOptions),
                validationResult.Failures);
        }

        return options;
    }

    /// <summary>
    /// Adds configuration from multiple directories to the configuration builder.
    /// </summary>
    /// <param name="configBuilder">The configuration builder to add directory configurations to.</param>
    /// <param name="dirPaths">Collection of directory paths to process for configuration files.</param>
    /// <remarks>
    /// For each directory path, this method creates a DirectoryInfo and calls AddDirectory
    /// with the isRootDir parameter set to true, indicating these are top-level directories.
    /// </remarks>
    private static void AddDirectories(
        IConfigurationBuilder configBuilder,
        IEnumerable<string> dirPaths)
    {
        foreach (var dirPath in dirPaths)
        {
            var dirInfo = new DirectoryInfo(dirPath);
            AddDirectory(configBuilder, dirInfo, isRootDir: true);
        }
    }

    /// <summary>
    /// Adds configuration from a directory to the configuration builder.
    /// </summary>
    /// <param name="parentConfigBuilder">The parent configuration builder to which the configuration will be added.</param>
    /// <param name="dirInfo">The directory information for the directory to process.</param>
    /// <param name="isRootDir">Indicates whether this is a root directory. If true, configurations are added directly;
    /// if false, they are added as nested sections.</param>
    /// <remarks>
    /// This method processes JSON files in the current directory and recursively processes subdirectories.
    /// For root directories, configuration is added directly to the parent builder.
    /// For nested directories, configuration is added as a named section.
    /// </remarks>
    private static void AddDirectory(
        IConfigurationBuilder parentConfigBuilder,
        DirectoryInfo dirInfo,
        bool isRootDir)
    {
        var dirConfigBuilder = new ConfigurationBuilder();
        AddJsonFilesFromDirectory(dirConfigBuilder, dirInfo);

        DirectoryInfo[] nestedDirs = dirInfo.GetDirectories(
            "*",
            SearchOption.TopDirectoryOnly);
        if (nestedDirs.Length > 0)
        {
            foreach (var dir in nestedDirs)
            {
                AddDirectory(dirConfigBuilder, dir, isRootDir: false);
            }
        }

        IConfigurationRoot dirConfig = dirConfigBuilder.Build();
        if (isRootDir)
        {
            parentConfigBuilder.AddConfiguration(dirConfig);
        }
        else
        {
            AddDirectoryAsSection(parentConfigBuilder, dirConfig, dirInfo);
        }
    }

    /// <summary>
    /// Adds a directory's configuration as a named section to a parent configuration builder.
    /// </summary>
    /// <param name="parentConfigBuilder">The parent configuration builder to which the section will be added.</param>
    /// <param name="dirConfig">The directory's configuration.</param>
    /// <param name="dirInfo">Directory information used to determine the section name.</param>
    /// <remarks>
    /// This method creates a section with the directory's name and adds all key-value pairs
    /// from the directory's configuration with appropriate nesting under that section name.
    /// </remarks>
    private static void AddDirectoryAsSection(
        IConfigurationBuilder parentConfigBuilder,
        IConfiguration dirConfig,
        DirectoryInfo dirInfo)
    {
        var sectionName = dirInfo.Name;
        parentConfigBuilder.AddInMemoryCollection(
        [
            new KeyValuePair<string, string?>(sectionName, null),
        ]);

        foreach (KeyValuePair<string, string?> kvp in dirConfig.AsEnumerable())
        {
            if (string.IsNullOrEmpty(kvp.Value))
            {
                continue;
            }

            var nestedKvp = new KeyValuePair<string, string?>(
                $"{sectionName}:{kvp.Key}",
                kvp.Value);
            parentConfigBuilder.AddInMemoryCollection([nestedKvp]);
        }
    }

    /// <summary>
    /// Adds JSON files from a specified directory to the configuration builder.
    /// </summary>
    /// <param name="configBuilder">The configuration builder to add JSON files to.</param>
    /// <param name="dirInfo">Directory information for the location containing JSON files to be added.</param>
    /// <remarks>
    /// This method searches for all JSON files in the specified directory (without recursion),
    /// and adds each file to the configuration builder using AddJsonFiles.
    /// If no JSON files are found, the method returns without making changes.
    /// </remarks>
    private static void AddJsonFilesFromDirectory(
        IConfigurationBuilder configBuilder,
        DirectoryInfo dirInfo)
    {
        FileInfo[] files = dirInfo.GetFiles(
            searchPattern: "*.json",
            searchOption: SearchOption.TopDirectoryOnly);

        if (files.Length is 0)
        {
            return;
        }

        IEnumerable<string> filePaths = files.Select(f => f.FullName);
        AddJsonFiles(configBuilder, filePaths);
    }

    /// <summary>
    /// Adds multiple JSON files to the configuration builder.
    /// </summary>
    /// <param name="configBuilder">The configuration builder to add JSON files to.</param>
    /// <param name="filePaths">Collection of file paths pointing to JSON configuration files.</param>
    /// <remarks>
    /// Each JSON file will be added with reload on change enabled and will be required (not optional).
    /// </remarks>
    private static void AddJsonFiles(
        IConfigurationBuilder configBuilder,
        IEnumerable<string> filePaths)
    {
        foreach (var filepath in filePaths)
        {
            configBuilder.AddJsonFile(
                filepath,
                optional: false,
                reloadOnChange: true);
        }
    }
}

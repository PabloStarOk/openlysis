using System.Collections.Immutable;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

using MimeDetective;
using MimeDetective.Definitions;
using MimeDetective.Definitions.Licensing;
using MimeDetective.Storage;

using Openlysis.Analyzers.HybridAnalysis.Core.Abstractions.Files;
using Openlysis.Analyzers.HybridAnalysis.Core.Configuration.Files;

namespace Openlysis.Analyzers.HybridAnalysis.Infrastructure.Files;

/// <summary>
/// Provides dependency injection extension methods for file type detection services.
/// </summary>
internal static class DependencyInjection
{
    /// <summary>
    /// Registers file type detection services in the dependency injection container.
    /// </summary>
    /// <param name="services">The service collection to add the services to.</param>
    /// <param name="configuration">The configuration containing file type detector settings.</param>
    internal static void AddFileTypeDetector(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddRequiredOptions(configuration);
        services.AddContentInspector();
        services.AddSingleton<IMimeTypeDetector, MimeTypeDetector>();
    }

    /// <summary>
    /// Adds and validates configuration options required for the file type detector.
    /// </summary>
    /// <param name="services">The service collection to add options to.</param>
    /// <param name="configuration">The configuration containing file type detector settings.</param>
    private static void AddRequiredOptions(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // Check options
        configuration.GetRequiredSection(FileTypeDetectorOptions.SectionName);

        // Add options
        services
            .AddOptions<FileTypeDetectorOptions>()
            .BindConfiguration(FileTypeDetectorOptions.SectionName)
            .ValidateDataAnnotations()
            .ValidateOnStart();
    }

    /// <summary>
    /// Registers the content inspector service in the dependency injection container.
    /// </summary>
    /// <param name="services">The service collection to add the content inspector to.</param>
    /// <remarks>
    /// This extension method configures a singleton instance of <see cref="IContentInspector"/>
    /// with definitions for file type detection.
    /// </remarks>
    private static void AddContentInspector(
        this IServiceCollection services)
    {
        ImmutableArray<Definition> definitions = new ExhaustiveBuilder
        {
            UsageType = UsageType.PersonalNonCommercial,
        }.Build();

        var inspector = new ContentInspectorBuilder
        {
            Definitions = definitions,
            Parallel = true,
        }.Build();

        services.AddSingleton(inspector);
    }
}
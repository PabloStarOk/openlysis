using System.ComponentModel.DataAnnotations;

using Microsoft.Extensions.Options;

namespace Openlysis.Infrastructure.Shared.Communication.Configuration;

/// <summary>
/// Represents configuration options for multiple message consumers.
/// </summary>
public sealed record ConsumersOptions
{
    /// <summary>
    /// The configuration section name for consumers options.
    /// </summary>
    public const string SectionName = nameof(ConsumersOptions);

    /// <summary>
    /// Gets configuration options for the file analysis consumer.
    /// </summary>
    [Required]
    [ValidateObjectMembers]
    required public ConsumerConfig AnalyzeFile { get; init; }

    /// <summary>
    /// Gets or sets configuration options for the URL analysis consumer.
    /// </summary>
    [Required]
    [ValidateObjectMembers]
    required public ConsumerConfig AnalyzeUrl { get; set; }

    /// <summary>
    /// Gets or sets configuration options for the file analysis update consumer.
    /// </summary>
    [Required]
    [ValidateObjectMembers]
    required public ConsumerConfig UpdateFileAnalysis { get; set; }

    /// <summary>
    /// Gets or sets configuration options for the URL analysis update consumer.
    /// </summary>
    [Required]
    [ValidateObjectMembers]
    required public ConsumerConfig UpdateUrlAnalysis { get; set; }

    /// <summary>
    /// Gets or sets configuration options for the message analysis update consumer.
    /// </summary>
    [Required]
    [ValidateObjectMembers]
    required public ConsumerConfig MessageAnalysisUpdate { get; set; }
}
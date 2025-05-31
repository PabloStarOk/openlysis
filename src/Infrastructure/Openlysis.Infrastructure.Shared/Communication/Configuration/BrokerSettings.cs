using System.ComponentModel.DataAnnotations;

namespace Openlysis.Infrastructure.Shared.Communication.Configuration;

/// <summary>
/// Represents the settings required to configure the broker connection.
/// </summary>
public record BrokerSettings
{
    /// <summary>
    /// The configuration section name for broker options.
    /// </summary>
    public const string SectionName = "BrokerSettings";

    /// <summary>
    /// Gets the scheme of the message broker.
    /// </summary>
    [Required]
    required public string Scheme { get; init; }

    /// <summary>
    /// Gets the host name of the RabbitMQ server.
    /// </summary>
    [Required]
    required public string Host { get; init; }

    /// <summary>
    /// Gets the port number of the RabbitMQ server.
    /// </summary>
    [Range(1, ushort.MaxValue)]
    required public ushort Port { get; init; }

    /// <summary>
    /// Gets the virtual host to use when connecting to the RabbitMQ server.
    /// </summary>
    [Required]
    required public string VirtualHost { get; init; }

    /// <summary>
    /// Gets the username to use when connecting to the RabbitMQ server.
    /// </summary>
    [Required]
    required public string Username { get; init; }

    /// <summary>
    /// Gets the password to use when connecting to the RabbitMQ server.
    /// </summary>
    [Required]
    required public string Password { get; init; }

    /// <summary>
    /// Gets the endpoint name for analyzing files.
    /// </summary>
    [Required]
    required public string AnalyzeFileEndpointName { get; init; }

    /// <summary>
    /// Gets the endpoint name for analyzing URLs.
    /// </summary>
    [Required]
    required public string AnalyzeUrlEndpointName { get; init; }

    /// <summary>
    /// Gets the endpoint name for updating file analysis.
    /// </summary>
    [Required]
    required public string UpdateFileAnalysisEndpointName { get; init; }

    /// <summary>
    /// Gets the endpoint name for updating URL analysis.
    /// </summary>
    [Required]
    required public string UpdateUrlAnalysisEndpointName { get; init; }
}
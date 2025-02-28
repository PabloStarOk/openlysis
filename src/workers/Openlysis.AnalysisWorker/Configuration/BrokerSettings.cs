namespace Openlysis.AnalysisWorker.Configuration;

/// <summary>
/// Represents the settings required to configure the broker connection.
/// </summary>
public record BrokerSettings
{
    /// <summary>
    /// The configuration section name for RabbitMQ options.
    /// </summary>
    public const string SectionName = "RabbitMQ";

    /// <summary>
    /// Gets the host name of the RabbitMQ server.
    /// </summary>
    public string Host { get; init; }

    /// <summary>
    /// Gets the port number of the RabbitMQ server.
    /// </summary>
    public ushort Port { get; init; }

    /// <summary>
    /// Gets the virtual host to use when connecting to the RabbitMQ server.
    /// </summary>
    public string VirtualHost { get; init; }

    /// <summary>
    /// Gets the username to use when connecting to the RabbitMQ server.
    /// </summary>
    public string Username { get; init; }

    /// <summary>
    /// Gets the password to use when connecting to the RabbitMQ server.
    /// </summary>
    public string Password { get; init; }
}
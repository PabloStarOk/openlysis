using System.Text.Json;

namespace Doppler.NET.Configuration;

/// <summary>
/// Options for configuring the Doppler client.
/// </summary>
public record DopplerClientOptions
{
    /// <summary>
    /// The configuration section name for Doppler client options.
    /// </summary>
    public const string SectionName = "DopplerClient";

    /// <summary>
    /// Gets or sets the name of the environment variable containing the Doppler service token.
    /// </summary>
    public string ServiceTokenEnvVariable { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the Doppler project name.
    /// </summary>
    public string ProjectName { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the Doppler configuration name.
    /// </summary>
    public string ConfigName { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the <see cref="JsonSerializerOptions"/> used for serializing and deserializing JSON data.
    /// </summary>
    public JsonSerializerOptions SerializerOptions { get; set; } = new ()
    {
        TypeInfoResolver = DopplerSerializerContext.Default,
        PropertyNameCaseInsensitive = true,
        PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower,
    };
}
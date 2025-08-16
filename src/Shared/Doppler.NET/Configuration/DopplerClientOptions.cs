using System.Text.Json;

namespace Doppler.NET.Configuration;

/// <summary>
/// Options for configuring the Doppler client.
/// </summary>
public record DopplerClientOptions
{
    /// <summary>
    /// Gets or sets the service token used for authentication with Doppler.
    /// </summary>
    required public string ServiceToken { get; set; }

    /// <summary>
    /// Gets or sets the Doppler project name.
    /// </summary>
    required public string ProjectName { get; set; }

    /// <summary>
    /// Gets or sets the Doppler configuration name.
    /// </summary>
    required public string ConfigName { get; set; }

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
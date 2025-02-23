namespace Filescan.Client.Models.Options;

/// <summary>
/// Represents the API options containing the API key.
/// </summary>
/// <param name="HttpVersion">Version of HTTP for requests.</param>
/// <param name="TimeoutPerRequest">A <see cref="TimeSpan"/> timeout for each request.</param>
/// <param name="ApiKey">API Key of Filescan.IO.</param>
public record ApiOptions(
    Version HttpVersion,
    TimeSpan TimeoutPerRequest,
    string ApiKey)
{
    /// <summary>
    /// Creates an instance of <see cref="ApiOptions"/> with the specified API key and timeout.
    /// </summary>
    /// <param name="apiKey">The API key for Filescan.IO.</param>
    /// <param name="msTimeout">The timeout for each request in milliseconds.</param>
    /// <returns>A new instance of <see cref="ApiOptions"/>.</returns>
    public static ApiOptions Create(string apiKey, int msTimeout)
    {
        return new ApiOptions(
            System.Net.HttpVersion.Version20,
            TimeSpan.FromMilliseconds(msTimeout),
            apiKey);
    }
}
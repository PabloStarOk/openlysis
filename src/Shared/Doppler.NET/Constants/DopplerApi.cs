namespace Doppler.NET.Constants;

/// <summary>
/// Constants for Doppler API.
/// </summary>
internal static class DopplerApi
{
    /// <summary>
    /// The authentication scheme used for Doppler API requests.
    /// </summary>
    public const string AuthScheme = "Bearer";

    /// <summary>
    /// The base URL for the Doppler API.
    /// </summary>
    private const string Url = "https://api.doppler.com/";

    /// <summary>
    /// Contains constants for Doppler API v3 endpoints.
    /// </summary>
    internal static class V3
    {
        /// <summary>
        /// The base URL for Doppler API v3.
        /// </summary>
        public const string BaseUrl = $"{Url}v3/";

        /// <summary>
        /// Contains endpoint paths related to secrets in Doppler API v3.
        /// </summary>
        public static class Secrets
        {
            /// <summary>
            /// The path for retrieving a specific secret by project, config, and name.
            /// </summary>
            public const string RetrievePath = "configs/config/secret?project={0}&config={1}&name={2}";
        }
    }
}
using Microsoft.Extensions.Options;

namespace Openlysis.Authentication.API.Endpoints.OpenID;

/// <summary>
/// Provides extension methods to map the OpenID configuration endpoint.
/// </summary>
internal static class OpenIdConfigurationEndpoint
{
    private const string Tag = "OpenID";
    private const string Route = "openid-configuration";
    private const string Name = "GetOpenIdConfiguration";

    /// <summary>
    /// Maps the OpenID configuration endpoint to the specified endpoint route builder.
    /// </summary>
    /// <param name="builder">The endpoint route builder to map the endpoint to.</param>
    public static void MapOpenIdConfigurationEndpoint(
        this IEndpointRouteBuilder builder)
    {
        builder.MapGet(Route, Handle)
            .AllowAnonymous()
            .Produces<OpenIdConfiguration>()
            .WithTags(Tag)
            .WithName(Name);
    }

    private static OpenIdConfiguration Handle(IOptions<OpenIdConfiguration> options)
    {
        return options.Value;
    }
}
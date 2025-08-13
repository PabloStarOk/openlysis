using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

using Openlysis.Authentication.API.Application.Common.Services;

namespace Openlysis.Authentication.API.Endpoints.OpenID;

/// <summary>
/// Provides endpoint mapping for the OpenID Connect JSON Web Keys (JWKS) endpoint.
/// </summary>
internal static class JsonWebKeysEndpoint
{
    private const string Name = "GetJwks";

    /// <summary>
    /// Maps the JWKS endpoint to the route defined in the OpenIdConfiguration.
    /// </summary>
    /// <param name="builder">The endpoint route builder.</param>
    /// <param name="serviceProvider">The service provider for resolving dependencies.</param>
    public static void MapJsonWebKeysEndpoint(
        this IEndpointRouteBuilder builder,
        IServiceProvider serviceProvider)
    {
        var options = serviceProvider.GetRequiredService<IOptions<OpenIdConfiguration>>();
        var route = options.Value.JwksUri.Segments
            .Last()
            .Trim(Uri.SchemeDelimiter.ToCharArray());

        builder.MapGet(route, Handle)
            .AllowAnonymous()
            .Produces<JsonWebKeySet>()
            .WithTags(OpenIdEndpointsConstants.Tag)
            .WithName(Name);
    }

    private static JsonWebKeySet Handle(IJwkProvider keyProvider)
    {
        return keyProvider.GetJsonWebKeySet();
    }
}
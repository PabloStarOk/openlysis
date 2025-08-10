using ErrorOr;

using FastEndpoints;

using Openlysis.Authentication.API.Application.Common.Models;
using Openlysis.Authentication.API.Application.Refresh;

namespace Openlysis.Authentication.API.Endpoints.Refresh;

/// <summary>
/// Endpoint for refreshing authentication tokens using a refresh token.
/// </summary>
internal sealed class SignInRefreshEndpoint : Endpoint<SignInRefreshRequest, AuthTokens>
{
    /// <summary>
    /// Service responsible for handling sign-in token refresh logic.
    /// </summary>
    private readonly ISignInTokenRefreshService _signInRefreshService;

    /// <summary>
    /// Initializes a new instance of the <see cref="SignInRefreshEndpoint"/> class.
    /// </summary>
    /// <param name="signInRefreshService">The service to handle token refresh operations.</param>
    public SignInRefreshEndpoint(ISignInTokenRefreshService signInRefreshService)
    {
        _signInRefreshService = signInRefreshService;
    }

    /// <inheritdoc/>
    public override void Configure()
    {
        Post("sign-in-refresh");
        AllowAnonymous();
        Description(
            builder =>
            {
                builder.Accepts<SignInRefreshRequest>("application/json");
                builder.Produces<AuthTokens>(statusCode: 200);
                builder.Produces(statusCode: 401);
                builder.ProducesValidationProblem();
            },
            clearDefaults: true);
        Summary(new SignInRefreshEndpointSummary());
    }

    /// <inheritdoc/>
    public override async Task HandleAsync(SignInRefreshRequest req, CancellationToken ct)
    {
        ErrorOr<AuthTokens> result =
            await _signInRefreshService.RefreshAsync(req.RefreshToken);

        if (result.IsError)
        {
            await Send.UnauthorizedAsync(CancellationToken.None);
            return;
        }

        var authTokens = result.Value;
        await Send.OkAsync(authTokens, CancellationToken.None);
    }
}
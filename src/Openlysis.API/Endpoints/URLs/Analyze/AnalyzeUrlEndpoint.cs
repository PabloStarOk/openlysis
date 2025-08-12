using System.Diagnostics.CodeAnalysis;
using System.Security.Claims;

using ErrorOr;

using FastEndpoints;

using Openlysis.API.Endpoints.URLs.GetAnalysisById;
using Openlysis.Application.URLs.Services;
using Openlysis.Domain.Common.ValueObjects;
using Openlysis.Domain.URLs;

namespace Openlysis.API.Endpoints.URLs.Analyze;

/// <summary>
/// Endpoint for analyzing URLs.
/// </summary>
/// <remarks>
/// This endpoint handles the analysis of URLs by validating the request,
/// retrieving the user ID, creating and sending the analysis command,
/// and finally creating the response.
/// </remarks>
public class AnalyzeUrlEndpoint : Endpoint<AnalyzeUrlRequest, AnalyzeUrlResponse>
{
    private const string Name = "AnalyzeUrl";

    private static readonly string DefaultScheme = Uri.UriSchemeHttps;
    private readonly ILogger<AnalyzeUrlEndpoint> _logger;
    private readonly IUrlMultiAnalysisService _multiAnalysisService;

    /// <summary>
    /// Initializes a new instance of the <see cref="AnalyzeUrlEndpoint"/> class.
    /// </summary>
    /// <param name="logger">The logger instance for logging information and errors.</param>
    /// <param name="multiAnalysisService">The service responsible for analyzing URLs.</param>
    public AnalyzeUrlEndpoint(
        ILogger<AnalyzeUrlEndpoint> logger,
        IUrlMultiAnalysisService multiAnalysisService)
    {
        _logger = logger;
        _multiAnalysisService = multiAnalysisService;
    }

    /// <inheritdoc/>
    public override void Configure()
    {
        Post(string.Empty);
        Group<UrlAnalysesGroup>();
        Version(1);
        Description(
            builder =>
            {
                builder.WithName(Name);
                builder.WithDisplayName(Name);
                builder.Accepts<AnalyzeUrlRequest>("application/x-www-form-urlencoded");
                builder.Produces<AnalyzeUrlResponse>();
                builder.ProducesValidationProblem();
                builder.ProducesProblem(StatusCodes.Status500InternalServerError);
            },
            clearDefaults: true);
        Summary(
            s =>
            {
                s.Summary = "Uploads a URL";
                s.Description = "Uploads a URL to be analyzed by multiple services.";
                s.ExampleRequest = new AnalyzeUrlRequest("https://example-site.com");
                s.RequestParam(r => r.Url, "URL to be analyzed.");
                s.RequestParam(r => r.Reanalyze, "Indicates whether the URL should be reanalyzed even if an existing analysis is available. Default is false.");
                s.RequestParam(r => r.IsPrivate, "If the analysis is only available to the user who uploads the URL. Default is false");
            });
    }

    /// <inheritdoc/>
    public override async Task HandleAsync(AnalyzeUrlRequest req, CancellationToken ct)
    {
        Claim userIdClaim = HttpContext.User.Claims.Single(c => c.Type is ClaimTypes.NameIdentifier);
        var userId = GlobalId.Parse(userIdClaim.Value);

        if (!TryCreateUri(req.Url, out Uri? url))
        {
            IResult badRequest = Results.Problem(
                statusCode: StatusCodes.Status400BadRequest,
                detail: "The URL is invalid.");
            await SendResultAsync(badRequest);
            return;
        }

        ErrorOr<UrlMultiAnalysis> result = await _multiAnalysisService
            .AnalyzeAsync(userId, req.IsPrivate, url, req.Reanalyze, ct);

        if (result.IsError)
        {
            IResult internalError = Results.Problem(
                statusCode: StatusCodes.Status500InternalServerError,
                detail: "Internal error while validating user, try again later.");

            _logger.LogError(
                "An error occurred while sending AnalyzeUrlCommand with mediator.\nErrors:\n\t{Errors}",
                result.Errors);
            await SendResultAsync(internalError);
            return;
        }

        Response = AnalyzeUrlResponse.Parse(result.Value);

        var routeValues = new RouteValueDictionary
        {
            { "id", Response.Id },
        };
        IResult accepted = Results.AcceptedAtRoute(GetAnalysisByIdEndpoint.Name, routeValues, Response);
        await SendResultAsync(accepted);
    }

    /// <summary>
    /// Tries to create a URI from the given source string.
    /// </summary>
    /// <param name="source">The source string to create the URI from.</param>
    /// <param name="url">The resulting URI if the creation is successful.</param>
    /// <returns>True if the URI is successfully created; otherwise, false.</returns>
    private static bool TryCreateUri(string source, [NotNullWhen(true)] out Uri? url)
    {
        if (!Uri.TryCreate(source, UriKind.RelativeOrAbsolute, out url))
        {
            return false;
        }

        if (url.IsAbsoluteUri)
        {
            return true;
        }

        var uriBuilder = new UriBuilder(DefaultScheme, url.OriginalString);
        try
        {
            url = uriBuilder.Uri;
            return true;
        }
        catch (UriFormatException)
        {
            return false;
        }
    }
}
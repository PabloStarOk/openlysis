using ErrorOr;

using FastEndpoints;

using MediatR;

using Openlysis.Application.FileAnalyses.Commands;

namespace Openlysis.API.Endpoints.Analyses.File.Analyze;

/// <summary>
/// Endpoint to analyze a file.
/// </summary>
public class Endpoint : Endpoint<Request, Response>
{
    private readonly IMediator _mediator;

    /// <summary>
    /// Gets the name of the endpoint.
    /// </summary>
    public static string Name { get; } = "AnalyzeFile";

    /// <summary>
    /// Initializes a new instance of the <see cref="Endpoint"/> class.
    /// </summary>
    /// <param name="mediator">Mediator to send commands and receive responses to application layer.</param>
    public Endpoint(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Configures the endpoint.
    /// </summary>
    public override void Configure()
    {
        Post(string.Empty);
        Group<FileAnalysesGroup>();
        AllowFileUploads();
        Description(
            b =>
            {
                b.WithName(Name);
                b.WithDisplayName(Name);
                b.Accepts<Request>(contentType: "multipart/form-data");
                b.Produces<Response>(StatusCodes.Status201Created);
                b.ProducesProblemDetails();
                b.ProducesProblemDetails(StatusCodes.Status500InternalServerError);
            });
        Summary(
            s =>
            {
                s.Summary = "Uploads a file.";
                s.Description = "Uploads a file to be analyzed.";
            });
    }

    /// <summary>
    /// Handles the file analysis request.
    /// </summary>
    /// <param name="request">The request containing the file to be analyzed.</param>
    /// <param name="ct">The cancellation token.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    public override async Task HandleAsync(Request request, CancellationToken ct)
    {
        if (request.File.Length <= 0)
        {
            await SendResultAsync(Results.Problem(
                statusCode: StatusCodes.Status400BadRequest,
                detail: "Provided file has no content."));
        }

        await using var fileData = request.File.OpenReadStream();
        var command = new AnalyzeFileCommand(
            request.File.FileName,
            request.File.ContentType,
            fileData,
            request.FileDescription,
            request.FilePassword,
            request.IsPrivateFile,
            request.Reanalyze);

        var fileAnalysisOrError = await _mediator.Send(command, ct);

        if (fileAnalysisOrError.IsError)
        {
            if (fileAnalysisOrError.Errors.Any(e => e.Type is ErrorType.Unexpected))
            {
                await SendResultAsync(Results.Problem(
                    statusCode: StatusCodes.Status500InternalServerError,
                    detail: "An internal error occured, try again later."));
            }

            var extensions = new Dictionary<string, object?>
            {
                {
                    "errors", fileAnalysisOrError.Errors
                },
            };
            await SendResultAsync(Results.Problem(
                statusCode: StatusCodes.Status400BadRequest,
                detail: "One or more errors occurred.",
                extensions: extensions));
        }

        Response = new Response(
            fileAnalysisOrError.Value.Id.Value.ToString(),
            fileAnalysisOrError.Value.ContentHashSet.Md5,
            fileAnalysisOrError.Value.ContentHashSet.Sha1,
            fileAnalysisOrError.Value.ContentHashSet.Sha256,
            fileAnalysisOrError.Value.ContentHashSet.Sha512);

        var routeValues = new Dictionary<string, string>
        {
            { "id", Response.FileAnalysisId },
        };

        await SendCreatedAtAsync(
            GetById.Endpoint.Name,
            routeValues,
            Response,
            generateAbsoluteUrl: true,
            ct);
    }
}
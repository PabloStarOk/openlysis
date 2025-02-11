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
                b.WithName("AnalyzeFile");
                b.WithDisplayName("AnalyzeFile");
                b.Accepts<Request>(contentType: "multipart/form-data");
                b.Produces<Response>(StatusCodes.Status201Created);
                b.ProducesProblemDetails();
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
            request.Reanalyze);

        var fileAnalysisOrError = await _mediator.Send(command, ct);

        Response = new Response(
            fileAnalysisOrError.Value.Id.Value.ToString(),
            fileAnalysisOrError.Value.File.HashSet.Md5,
            fileAnalysisOrError.Value.File.HashSet.Sha1,
            fileAnalysisOrError.Value.File.HashSet.Sha256,
            fileAnalysisOrError.Value.File.HashSet.Sha512);

        var routeValues = new Dictionary<string, string>
        {
            { "id", Response.FileAnalysisId },
        };

        await SendCreatedAtAsync(
            "GetFileAnalysisById",
            routeValues,
            Response,
            generateAbsoluteUrl: true,
            ct);
    }
}
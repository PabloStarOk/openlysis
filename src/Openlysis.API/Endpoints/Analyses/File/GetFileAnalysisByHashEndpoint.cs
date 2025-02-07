using ErrorOr;

using MediatR;

using Microsoft.AspNetCore.Mvc;

using Openlysis.Application.FileAnalyses.Queries;
using Openlysis.Domain.FileAnalyses;

namespace Openlysis.API.Endpoints.Analyses.File;

/// <summary>
/// Endpoint for retrieving file analysis by hash.
/// </summary>
public static class GetFileAnalysisByHashEndpoint
{
    /// <summary>
    /// Maps the endpoint route to the handler method.
    /// </summary>
    /// <param name="group">The endpoint route builder.</param>
    public static void Map(IEndpointRouteBuilder group)
    {
        // TODO: Add metadata and security.
        group.MapGet("/hash/{hash}", GetFileAnalysisByHash)
            .WithName("GetFileAnalysisByHash");
    }

    /// <summary>
    /// Handles the request to get file analysis by hash.
    /// </summary>
    /// <param name="mediator">The mediator for sending queries.</param>
    /// <param name="hash">The hash of the file to analyze.</param>
    /// <returns>The result of the file analysis.</returns>
    private static async Task<IResult> GetFileAnalysisByHash(
        [FromServices] IMediator mediator,
        [FromRoute] string hash)
    {
        var query = new FileAnalysisQueryByHash(hash);

        ErrorOr<FileAnalysis> fileAnalysis = await mediator.Send(query);

        return fileAnalysis.IsError
            ? Results.BadRequest(fileAnalysis.FirstError) // TODO: Return ProblemsDetails.
            : Results.Ok(fileAnalysis.Value); // TODO: Return DTO.
    }
}
using ErrorOr;

using MediatR;

using Microsoft.AspNetCore.Mvc;

using Openlysis.Application.FileAnalyses.Queries;
using Openlysis.Domain.FileAnalyses;
using Openlysis.Domain.FileAnalyses.ValueObjects;

namespace Openlysis.API.Endpoints.Analyses.File;

/// <summary>
/// Endpoint to get a file analysis by its ID.
/// </summary>
public static class GetFileAnalysisByIdEndpoint
{
    /// <summary>
    /// Maps the endpoint to get a file analysis by its ID.
    /// </summary>
    /// <param name="group">The endpoint route builder.</param>
    public static void Map(IEndpointRouteBuilder group)
    {
        // TODO: Add metadata and security.
        group.MapGet("{id}", GetFileAnalysisById)
            .WithName("GetFileAnalysisById");
    }

    /// <summary>
    /// Handles the request to get a file analysis by its ID.
    /// </summary>
    /// <param name="mediator">The mediator instance to send the query.</param>
    /// <param name="id">The ID of the file analysis.</param>
    /// <returns>An <see cref="IResult"/> containing the file analysis or an error.</returns>
    private static async Task<IResult> GetFileAnalysisById(
        [FromServices] IMediator mediator,
        [FromRoute] string id)
    {
        var fileAnalysisId = FileAnalysisId.Create(Guid.Parse(id));
        var query = new FileAnalysisQueryById(fileAnalysisId);

        ErrorOr<FileAnalysis> fileAnalysis = await mediator.Send(query);

        return fileAnalysis.IsError
            ? Results.BadRequest(fileAnalysis.FirstError) // TODO: Return ProblemsDetails.
            : Results.Ok(fileAnalysis.Value); // TODO: Return DTO.
    }
}
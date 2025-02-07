using Openlysis.API.Endpoints.Analyses.File;

namespace Openlysis.API.Endpoints.Analyses;

/// <summary>
/// Group of endpoints for file analyses.
/// </summary>
public static class FileAnalysesGroup
{
    /// <summary>
    /// Add all endpoints related to file analyses.
    /// </summary>
    /// <param name="routeBuilder">Builder of routes.</param>
    public static void AddFileAnalysisEndpoints(this IEndpointRouteBuilder routeBuilder)
    {
        // TODO: Add metadata and security.
        var group = routeBuilder.MapGroup("/api/v1/analyses/file/")
            .WithName("FileAnalyses");

        GetFileAnalysisByIdEndpoint.Map(group);
        GetFileAnalysisByHashEndpoint.Map(group);
    }
}
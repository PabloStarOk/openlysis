using FastEndpoints;

using Openlysis.API.Endpoints.Analyses.File;

namespace Openlysis.API.Endpoints.Analyses;

/// <summary>
/// Group of endpoints for file analyses.
/// </summary>
public sealed class FileAnalysesGroup : Group
{
    /// <summary>
    /// Initializes a new instance of the <see cref="FileAnalysesGroup"/> class.
    /// </summary>
    public FileAnalysesGroup()
    {
        Configure("/api/v1/analyses/file", ep =>
        {
            ep.Description(b =>
                {
                    b.WithGroupName("FileAnalyses");
                    b.WithDisplayName("FileAnalyses");
                    b.WithTags("File");
                });
            ep.AllowAnonymous(); // TODO: Add and security.
        });
    }
}
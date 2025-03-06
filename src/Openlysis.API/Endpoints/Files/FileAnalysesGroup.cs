using FastEndpoints;

namespace Openlysis.API.Endpoints.Files;

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
        Configure("files", ep =>
        {
            ep.Description(b =>
                {
                    b.WithGroupName("FileMultiAnalyses");
                    b.WithDisplayName("FileMultiAnalyses");
                    b.WithTags("File");
                });
        });
    }
}
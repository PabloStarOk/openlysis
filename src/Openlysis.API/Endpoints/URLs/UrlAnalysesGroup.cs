using FastEndpoints;

namespace Openlysis.API.Endpoints.URLs;

/// <summary>
/// Represents a group of URL analyses endpoints.
/// </summary>
public class UrlAnalysesGroup : Group
{
    /// <summary>
    /// Initializes a new instance of the <see cref="UrlAnalysesGroup"/> class.
    /// Configures the group with the base path "urls".
    /// </summary>
    public UrlAnalysesGroup()
    {
        Configure("urls", ep =>
        {
            ep.Description(
                b =>
                {
                    b.WithGroupName("Urls");
                    b.WithDisplayName("Urls");
                    b.WithTags("Urls");
                });
        });
    }
}
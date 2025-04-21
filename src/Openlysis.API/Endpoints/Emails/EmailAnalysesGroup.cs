using FastEndpoints;

namespace Openlysis.API.Endpoints.Emails;

/// <summary>
/// Represents a group of endpoints related to email analyses.
/// </summary>
public sealed class EmailAnalysesGroup : Group
{
    /// <summary>
    /// Initializes a new instance of the <see cref="EmailAnalysesGroup"/> class
    /// and configures the group with metadata for email-related endpoints.
    /// </summary>
    public EmailAnalysesGroup()
    {
        Configure("emails", ep =>
        {
            ep.Description(
                b =>
                {
                    b.WithGroupName("Emails");
                    b.WithDisplayName("Emails");
                    b.WithTags("Emails");
                });
        });
    }
}
using FastEndpoints;

namespace Openlysis.API.Endpoints.Messages;

/// <summary>
/// Represents a group of endpoints related to message analyses.
/// </summary>
public sealed class MessageAnalysesGroup : Group
{
    /// <summary>
    /// Initializes a new instance of the <see cref="MessageAnalysesGroup"/> class
    /// and configures the group with metadata for message-related endpoints.
    /// </summary>
    public MessageAnalysesGroup()
    {
        Configure("messages", ep =>
        {
            ep.Description(
                b =>
                {
                    b.WithGroupName("Messages");
                    b.WithDisplayName("Messages");
                });
        });
    }
}
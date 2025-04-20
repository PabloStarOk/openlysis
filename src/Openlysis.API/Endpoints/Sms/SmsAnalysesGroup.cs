using FastEndpoints;

namespace Openlysis.API.Endpoints.Sms;

/// <summary>
/// Represents a group of endpoints related to SMS analyses.
/// </summary>
public class SmsAnalysesGroup : Group
{
    /// <summary>
    /// Initializes a new instance of the <see cref="SmsAnalysesGroup"/> class
    /// and configures the group with the "sms" prefix and metadata.
    /// </summary>
    public SmsAnalysesGroup()
    {
        Configure("sms", ep =>
        {
            ep.Description(
                b =>
                {
                    b.WithGroupName("SMS");
                    b.WithDisplayName("SMS");
                    b.WithTags("SMS");
                });
        });
    }
}
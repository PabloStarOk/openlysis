using Openlysis.Domain.Messages;

namespace Openlysis.API.Endpoints.Messages.Common.Responses;

/// <summary>
/// Data extracted from a message's subject and content, including URLs, email addresses, and phone numbers.
/// </summary>
/// <param name="Urls">A collection of detected URLs.</param>
/// <param name="EmailAddresses">A collection of detected email addresses.</param>
/// <param name="PhoneNumbers">A collection of detected phone numbers.</param>
public record MessageDetectedData(
    IEnumerable<Uri> Urls,
    IEnumerable<string> EmailAddresses,
    IEnumerable<string> PhoneNumbers)
{
    /// <summary>
    /// Creates a new instance of <see cref="MessageDetectedData"/> from the provided <see cref="MessageAnalysis"/> source.
    /// </summary>
    /// <param name="source">The source object containing the results of message analysis.</param>
    /// <returns>A new <see cref="MessageDetectedData"/> instance populated with detected URLs, email addresses, and phone numbers.</returns>
    public static MessageDetectedData CreateFromMessageAnalysis(MessageAnalysis source)
    {
        IEnumerable<Uri> urls = source.DetectedUrlsResults
            .Select(d => d.Value);
        IEnumerable<string> emailAddresses = source.DetectedEmailAddressesResults
            .Select(d => d.Value.Address);
        IEnumerable<string> phoneNumbers = source.DetectedPhoneNumbersResults
            .Select(d => d.Value);

        return new MessageDetectedData(
            urls,
            emailAddresses,
            phoneNumbers);
    }
}
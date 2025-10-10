using System.Net.Mail;

namespace Openlysis.Application.Messages.Contracts.Abstractions;

/// <summary>
/// Defines methods for extracting specific types of data, such as URLs, email addresses, and phone numbers, from input strings.
/// </summary>
public interface IMessageDataExtractor
{
    /// <summary>
    /// Sets the country code to be used for formatting extractions.
    /// </summary>
    /// <param name="requestCountryCode">The country code in ISO 3166-1 alpha-2 format.</param>
    public void SetRequestCountryCode(string requestCountryCode);

    /// <summary>
    /// Extracts URLs from the given input string.
    /// </summary>
    /// <param name="input">The input string to extract URLs from.</param>
    /// <returns>An enumerable collection of extracted URLs as <see cref="Uri"/> objects.</returns>
    public IEnumerable<Uri> ExtractUrls(string input);

    /// <summary>
    /// Extracts email addresses from the given input string.
    /// </summary>
    /// <param name="input">The input string to extract email addresses from.</param>
    /// <returns>An enumerable collection of extracted email addresses as <see cref="MailAddress"/> objects.</returns>
    public IEnumerable<MailAddress> ExtractEmailAddresses(string input);

    /// <summary>
    /// Extracts phone numbers from the given input string.
    /// </summary>
    /// <param name="input">The input string to extract phone numbers from.</param>
    /// <returns>An enumerable collection of extracted phone numbers in E.164 format as strings.</returns>
    public IEnumerable<string> ExtractPhoneNumbers(string input);
}
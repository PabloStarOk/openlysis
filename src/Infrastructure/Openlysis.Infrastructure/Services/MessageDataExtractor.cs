using System.Net.Mail;

using Openlysis.Application.Messages.Contracts.Abstractions;
using Openlysis.Domain.Messages.Enums;

namespace Openlysis.Infrastructure.Services;

/// <summary>
/// An extractor of data for messages.
/// </summary>
internal sealed class MessageDataExtractor : IMessageDataExtractor
{
    private readonly IEnumerable<DataDetector> _dataDetectors;
    private string _requestCountryCode = string.Empty;

    /// <summary>
    /// Initializes a new instance of the <see cref="MessageDataExtractor"/> class.
    /// </summary>
    /// <param name="dataDetectors">A collection of <see cref="DataDetector"/> instances used to extract data from input.</param>
    public MessageDataExtractor(IEnumerable<DataDetector> dataDetectors)
    {
        _dataDetectors = dataDetectors;
    }

    /// <inheritdoc/>
    public void SetRequestCountryCode(string requestCountryCode)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(requestCountryCode);
        _requestCountryCode = requestCountryCode;
    }

    /// <inheritdoc/>
    public IEnumerable<Uri> ExtractUrls(string input)
    {
        IEnumerable<string> urls = ExtractData(DataType.Url, input);
        return urls.Select(u => new Uri(u));
    }

    /// <inheritdoc/>
    public IEnumerable<MailAddress> ExtractEmailAddresses(string input)
    {
        IEnumerable<string> emails = ExtractData(DataType.EmailAddress, input);
        return emails.Select(e => new MailAddress(e));
    }

    /// <inheritdoc/>
    public IEnumerable<string> ExtractPhoneNumbers(string input)
        => ExtractData(DataType.PhoneNumber, input);

    /// <summary>
    /// Extracts data of the specified type from the given input string.
    /// </summary>
    /// <param name="dataType">The type of data to extract (e.g., URL, email address, phone number).</param>
    /// <param name="input">The input string to process for data extraction.</param>
    /// <returns>A collection of extracted data as strings.</returns>
    private IEnumerable<string> ExtractData(
        DataType dataType,
        string input)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(input);

        var detector = _dataDetectors
            .Single(d => d.DetectableData == dataType);

        return detector.Detect(input, _requestCountryCode);
    }
}
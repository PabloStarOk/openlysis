using Microsoft.Extensions.Logging;

using Openlysis.Domain.Common.Enums;
using Openlysis.Domain.Phones.Entities;
using Openlysis.Domain.Phones.ValueObjects;
using Openlysis.TestTools.ServicesSimulation.Common.Services.Abstractions;
using Openlysis.TestTools.ServicesSimulation.PhonesNumbers.Configuration;

namespace Openlysis.TestTools.ServicesSimulation.PhonesNumbers.Infrastructure;

/// <summary>
/// Factory for creating stubbed <see cref="PhoneReputation"/> instances.
/// </summary>
/// <remarks>
/// This factory generates phone reputation data with configurable options
/// or random data when no specific options are provided.
/// </remarks>
internal sealed class PhoneReputationStubFactory
    : StubFactory<PhoneReputationStubFactoryOptions, PhoneReputation>
{
    private const ushort ExistingDiallingCodes = 250;
    private const ushort MinPhoneNumberDigits = 8;
    private const ushort MaxPhoneNumberDigits = 15;

    /// <summary>
    /// Initializes a new instance of the <see cref="PhoneReputationStubFactory"/> class.
    /// </summary>
    /// <param name="logger">The logger used for logging information and errors.</param>
    public PhoneReputationStubFactory(ILogger<PhoneReputationStubFactory> logger)
        : base(logger)
    {
    }

    /// <inheritdoc/>
    protected override PhoneReputation HandleCreation(
        string serviceName,
        PhoneReputationStubFactoryOptions options)
    {
        Verdict verdict = GenerateVerdict(options.VerdictSimulation);

        PhoneInfo phoneInfo = options.PhoneInfoStub is not null ?
            options.PhoneInfoStub with { } :
            GenerateRandomPhoneInfo();

        return PhoneReputation.Create(
            serviceName,
            verdict,
            phoneInfo);
    }

    /// <inheritdoc/>
    protected override void LogCreatedStub(PhoneReputation stub)
    {
        Logger.LogTrace(
            "{TypeName} created:"
            + "\n\tService name: {ServiceName}"
            + "\n\tID: {Id}"
            + "\n\tVerdict: {Verdict}"
            + "\n\tPhone number in local format: {LocalFormat}"
            + "\n\tCountry code: {CountryCode}"
            + "\n\tDialing code: {DialingCode}"
            + "\n\tLine type: {LineType}",
            nameof(PhoneReputation),
            stub.ServiceName,
            stub.Id,
            stub.Verdict,
            stub.PhoneInfo.LocalFormat,
            stub.PhoneInfo.CountryCode,
            stub.PhoneInfo.DialingCode,
            stub.PhoneInfo.LineType);
    }

    /// <summary>
    /// Generates a random <see cref="PhoneInfo"/> instance with randomly generated properties.
    /// </summary>
    /// <returns>A new <see cref="PhoneInfo"/> instance with random phone number, country code, dialing code, and line type.</returns>
    private static PhoneInfo GenerateRandomPhoneInfo()
    {
        string phoneNumber = GenerateRandomPhoneNumber();
        string countryCode = GenerateRandomCountryCode();
        int dialingCode = Random.Shared.Next(0, ExistingDiallingCodes + 1);
        LineType lineType = GetRandomValue(Enum.GetValues<LineType>());

        return new PhoneInfo(
            phoneNumber,
            countryCode,
            dialingCode,
            lineType.ToString());
    }

    /// <summary>
    /// Generates a random phone number string with a length between the defined minimum and maximum digits.
    /// </summary>
    /// <returns>A string representing a random phone number containing only digits.</returns>
    private static string GenerateRandomPhoneNumber()
    {
        List<int> possibleLengths = Enumerable
            .Range(MinPhoneNumberDigits, MaxPhoneNumberDigits)
            .ToList();
        int randomLength = GetRandomValue(possibleLengths);

        var digits = new List<int>(randomLength);
        for (int i = 0; i < randomLength; i++)
        {
            int randomDigit = Random.Shared.Next(0, 10);
            digits.Add(randomDigit);
        }

        return string.Join(string.Empty, digits);
    }

    /// <summary>
    /// Generates a random two-letter country code.
    /// </summary>
    /// <returns>A string representing a random two-letter country code (e.g. "US", "CA", "UK").</returns>
    private static string GenerateRandomCountryCode()
    {
        char[] alphabets = Enumerable.Range('A', 26).Select(i => (char)i).ToArray();
        char firstChar = GetRandomValue(alphabets);
        char secondChar = GetRandomValue(alphabets);
        return string.Join(string.Empty, firstChar, secondChar);
    }

    /// <summary>
    /// Represents the different types of phone lines that can be encountered.
    /// </summary>
    private enum LineType
    {
        /// <summary>Mobile phone line.</summary>
        Mobile,

        /// <summary>Toll-free phone line.</summary>
        TollFree,

        /// <summary>Wireless phone line.</summary>
        Wireless,

        /// <summary>Traditional landline phone.</summary>
        Landline,

        /// <summary>Satellite-based phone line.</summary>
        Satellite,

        /// <summary>Voice over IP phone line.</summary>
        Voip,

        /// <summary>Premium rate service phone line.</summary>
        PremiumRate,

        /// <summary>Pager service line.</summary>
        Pager,

        /// <summary>Phone line of unknown type.</summary>
        Unknown,
    }
}
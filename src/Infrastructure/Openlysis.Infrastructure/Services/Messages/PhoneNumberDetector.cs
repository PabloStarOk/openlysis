using System.Text.RegularExpressions;

using Microsoft.Extensions.Logging;

using Openlysis.Application.Messages.Contracts.Abstractions;
using Openlysis.Infrastructure.Persistence;

using PhoneNumbers;

using DataType = Openlysis.Domain.Messages.Enums.DataType;

namespace Openlysis.Infrastructure.Services.Messages;

/// <summary>
/// A service that detects phone numbers in a given input string.
/// </summary>
internal class PhoneNumberDetector : DataDetector
{
    /// <inheritdoc/>
    public override DataType DetectableData => DataType.PhoneNumber;

    private const string DefaultRegionCode = "CO";
    private const string UnknownRegionCode = "ZZ";

    private static readonly HashSet<int> CountryCodes =
        PhoneNumberUtil.GetInstance()
            .GetSupportedCallingCodes()
            .Order()
            .ToHashSet();

    private readonly PhoneNumberUtil _phoneNumberUtil;

    /// <summary>
    /// Initializes a new instance of the <see cref="PhoneNumberDetector"/> class.
    /// </summary>
    /// <param name="logger">
    /// The logger instance used for logging within the detector.
    /// It provides a mechanism to log information, warnings, and errors.
    /// </param>
    /// <param name="phoneNumberUtil">
    /// An instance of <see cref="PhoneNumberUtil"/> used for parsing, formatting,
    /// and validating phone numbers.
    /// </param>
    public PhoneNumberDetector(
        ILogger<PhoneNumberDetector> logger,
        PhoneNumberUtil phoneNumberUtil)
        : base(logger)
    {
        _phoneNumberUtil = phoneNumberUtil;
    }

    /// <inheritdoc/>
    protected override Regex GetDetectionPattern() => RegularExpressions.PhoneNumber();

    /// <inheritdoc/>
    protected override IEnumerable<Regex> GetExclusionPatterns() =>
        RegularExpressions.GetAllExcluding(GetDetectionPattern());

    /// <inheritdoc/>
    protected override string NormalizeDetection(string detection)
    {
        PhoneNumber phoneNumber;
        try
        {
            string regionCode = RequestCountryCode ?? DefaultRegionCode;
            phoneNumber = _phoneNumberUtil.Parse(detection, regionCode);
        }
        catch (NumberParseException)
        {
            return string.Empty;
        }

        phoneNumber = SetValidCountryCodeForNumber(phoneNumber);
        return _phoneNumberUtil.Format(phoneNumber, PhoneNumberFormat.E164);
    }

    /// <inheritdoc/>
    protected override bool ValidateDetection(string detection)
    {
        try
        {
            PhoneNumber phoneNumber = _phoneNumberUtil.Parse(
                detection,
                UnknownRegionCode);
            return _phoneNumberUtil.IsValidNumber(phoneNumber);
        }
        catch (NumberParseException)
        {
            return false;
        }
    }

    /// <summary>
    /// Sets a valid country code for the given phone number if the current country code is invalid.
    /// </summary>
    /// <param name="phoneNumber">The phone number to update with a valid country code.</param>
    /// <returns>
    /// A new <see cref="PhoneNumber"/> instance with a valid country code, or the default instance if no valid code is found.
    /// </returns>
    private PhoneNumber SetValidCountryCodeForNumber(PhoneNumber phoneNumber)
    {
        if (_phoneNumberUtil.IsValidNumber(phoneNumber))
        {
            return phoneNumber;
        }

        ulong nationalNumber = phoneNumber.NationalNumber;
        int? validRegionCode = CountryCodes
            .FirstOrDefault(c => IsValidCountryCodeForNumber(nationalNumber, c));

        if (validRegionCode is null)
        {
            return PhoneNumber.DefaultInstance;
        }

        return phoneNumber
            .ToBuilder()
            .SetCountryCode((int)validRegionCode)
            .Build();
    }

    /// <summary>
    /// Validates if the given country code is appropriate for the provided national number.
    /// </summary>
    /// <param name="nationalNumber">The national number to validate.</param>
    /// <param name="countryCode">The country code to test against the national number.</param>
    /// <returns>
    /// True if the combination of the national number and country code forms a valid phone number; otherwise, false.
    /// </returns>
    private bool IsValidCountryCodeForNumber(ulong nationalNumber, int countryCode)
    {
        var testNumber = new PhoneNumber()
            .ToBuilder()
            .SetNationalNumber(nationalNumber)
            .SetCountryCode(countryCode)
            .Build();

        return _phoneNumberUtil.IsValidNumber(testNumber);
    }
}
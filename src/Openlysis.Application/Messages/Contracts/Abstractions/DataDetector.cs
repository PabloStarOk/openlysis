using System.Diagnostics;
using System.Text.RegularExpressions;

using Microsoft.Extensions.Logging;

using Openlysis.Domain.Messages.Enums;

namespace Openlysis.Application.Messages.Contracts.Abstractions;

/// <summary>
/// Defines a detector of data.
/// </summary>
public abstract class DataDetector
{
    /// <summary>
    /// Gets the type of data that this detector can identify.
    /// </summary>
    public abstract DataType DetectableData { get; }

    /// <summary>
    /// Gets the code of a country in the ISO 3166 format.
    /// </summary>
    protected string? RequestCountryCode { get; private set; }

    private readonly ILogger<DataDetector> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="DataDetector"/> class.
    /// </summary>
    /// <param name="logger">The logger instance used for logging within the detector.</param>
    protected DataDetector(ILogger<DataDetector> logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// Detects data from the provided input string using the implemented detection logic.
    /// </summary>
    /// <param name="input">The input string to analyze for data detection. Must not be null or whitespace.</param>
    /// <param name="requestCountryCode">
    /// The ISO 3166 country code associated with the request. Must be a valid two-letter code.
    /// </param>
    /// <returns>
    /// A collection of detected strings that are valid, formatted, and unique.
    /// </returns>
    public IEnumerable<string> Detect(string input, string? requestCountryCode = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(input);
        RequestCountryCode = FormatCountryCode(requestCountryCode);

        string cleanedInput = CleanInput(input);
        string[] rawDetections = HandleDetectionCore(cleanedInput);
        var filteredDetections = ApplyExclusionFilters(rawDetections);

#if DEBUG
        LogDetectionsResults(rawDetections, filteredDetections.ToArray());
#endif

        return filteredDetections
            .Select(NormalizeDetection)
            .ToHashSet()
            .Where(ValidateDetection);
    }

    /// <summary>
    /// Allows derived classes to perform additional cleaning on the input string.
    /// This method can be overridden to apply custom cleaning logic.
    /// </summary>
    /// <param name="input">The input string to clean.</param>
    /// <returns>The cleaned input string after applying custom logic.</returns>
    protected virtual string OnCleanInput(string input)
    {
        return input;
    }

    /// <summary>
    /// Handles the detection process for the given input string.
    /// This method can be overridden to customize the detection logic.
    /// </summary>
    /// <param name="input">The input string to process for detection.</param>
    /// <returns>An array of detected strings based on the implemented logic.</returns>
    protected virtual string[] HandleDetectionCore(string input)
    {
        IEnumerable<Match> matches = FindMatches(input);

        return matches
            .Where(m => m.Success)
            .Select(m => m.Value)
            .ToArray();
    }

    /// <summary>
    /// Formats a detected string by applying any necessary transformations.
    /// The default implementation trims whitespace from the detection.
    /// </summary>
    /// <param name="detection">The detected string to format.</param>
    /// <returns>The formatted detection string.</returns>
    protected virtual string NormalizeDetection(string detection)
    {
        return detection.Trim();
    }

    /// <summary>
    /// Retrieves the regular expression pattern used for detecting data.
    /// Derived classes must implement this method to provide the specific detection logic.
    /// </summary>
    /// <returns>A <see cref="Regex"/> object representing the detection pattern.</returns>
    protected abstract Regex GetDetectionPattern();

    /// <summary>
    /// Retrieves a collection of regular expressions used to exclude unwanted patterns
    /// from the input data during detection.
    /// </summary>
    /// <remarks>
    /// These regex patterns are applied to filter out irrelevant or invalid data,
    /// ensuring the detection process focuses only on meaningful input.
    /// Derived classes should return a set of regex patterns specific to the data type being detected.
    /// </remarks>
    /// <returns>An enumerable collection of <see cref="Regex"/> objects for excluding unwanted patterns.</returns>
    protected abstract IEnumerable<Regex> GetExclusionPatterns();

    /// <summary>
    /// Validates a detected string based on custom logic implemented in derived classes.
    /// </summary>
    /// <param name="detection">The detected string to validate.</param>
    /// <returns>
    /// <c>true</c> if the detection meets the validation criteria; otherwise, <c>false</c>.
    /// </returns>
    protected abstract bool ValidateDetection(string detection);

    /// <summary>
    /// Validates and formats the provided country code to ensure it adheres to the ISO 3166 standard.
    /// </summary>
    /// <param name="countryCode">The ISO 3166 country code to validate and format. Must be exactly two letters.</param>
    /// <returns>The validated and formatted country code in uppercase, or <c>null</c> if the input is <c>null</c>.</returns>
    /// <exception cref="ArgumentException">
    /// Thrown if the country code is not exactly two letters or contains invalid characters.
    /// </exception>
    private static string? FormatCountryCode(string? countryCode)
    {
        if (countryCode is null)
        {
            return countryCode;
        }

        if (countryCode.Length is not 2
            || countryCode.Any(c => !char.IsLetter(c)))
        {
            throw new ArgumentException("Country code request must be in the ISO 3166 format.", nameof(countryCode));
        }

        return countryCode.ToUpper();
    }

    /// <summary>
    /// Cleans the input string by trimming leading and trailing periods and whitespace.
    /// </summary>
    /// <param name="input">The input string to clean.</param>
    /// <returns>The cleaned input string.</returns>
    private string CleanInput(string input)
    {
        return OnCleanInput(input)
            .Trim('.')
            .Trim();
    }

    /// <summary>
    /// Applies exclusion filters to the provided detections using the exclusion patterns.
    /// Filters out detections that match any of the exclusion patterns defined in derived classes.
    /// </summary>
    /// <param name="detections">The collection of detected strings to filter.</param>
    /// <returns>An array of detections that do not match any exclusion patterns.</returns>
    private string[] ApplyExclusionFilters(IEnumerable<string> detections)
    {
        try
        {
            IEnumerable<Regex> exclusionPatterns = GetExclusionPatterns();

            foreach (var regex in exclusionPatterns)
            {
                detections = detections
                    .Where(d => !regex.IsMatch(d));
            }

            return detections.ToArray();
        }
        catch (RegexMatchTimeoutException ex)
        {
            _logger.LogError(
                ex,
                "Regex timed out when filtering raw detections after {Time} milliseconds.",
                ex.MatchTimeout);

            return [];
        }
    }

    /// <summary>
    /// Finds matches in the provided input string using the detection pattern.
    /// </summary>
    /// <param name="input">The input string to search for matches.</param>
    /// <returns>
    /// A collection of <see cref="Match"/> objects representing the matches found in the input string.
    /// </returns>
    private IEnumerable<Match> FindMatches(string input)
    {
        try
        {
            return GetDetectionPattern().Matches(input);
        }
        catch (RegexMatchTimeoutException ex)
        {
            _logger.LogError(
                ex,
                "Regex timed out when detecting {DataType} after {Time} milliseconds for an input of {InputLength} characters.",
                DetectableData,
                ex.MatchTimeout,
                input.Length);

            return [];
        }
    }

#if DEBUG

    [Conditional("DEBUG")]
    private void LogDetectionsResults(
        string[] rawDetections,
        string[] filteredDetections)
    {
        _logger.LogDebug(
            "{RawCount} data found after a raw detection: {RawDetections}\n\t{FilteredCount} detections were filtered: {FilteredDetections}",
            rawDetections.Length,
            rawDetections,
            filteredDetections.Length,
            filteredDetections);
    }

#endif
}
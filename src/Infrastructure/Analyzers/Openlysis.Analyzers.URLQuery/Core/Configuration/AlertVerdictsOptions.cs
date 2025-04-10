using System.ComponentModel.DataAnnotations;

namespace Openlysis.Analyzers.URLQuery.Core.Configuration;

/// <summary>
/// Represents the options for alert verdicts.
/// </summary>
public record AlertVerdictsOptions
{
    private readonly string[] _malicious = [];
    private readonly string[] _suspicious = [];
    private readonly string[] _undetected = [];

    /// <summary>
    /// Gets the normalized malicious verdict strings that sensors could return.
    /// </summary>
    /// <remarks>
    /// The verdict strings are normalized by removing hyphens, underscores, spaces, and trimming whitespace.
    /// Converts the string to lowercase.
    /// </remarks>
    [MinLength(1, ErrorMessage = "There must be at least one malicious string")]
    required public string[] Malicious
    {
        get => _malicious;
        init
        {
            _malicious = value
                .Select(VerdictCalculationOptions.NormalizeAlertString)
                .ToArray();
        }
    }

    /// <summary>
    /// Gets the normalized suspicious verdicts strings that sensors could return.
    /// </summary>
    /// <remarks>
    /// The verdict strings are normalized by removing hyphens, underscores, spaces, and trimming whitespace.
    /// Converts the string to lowercase.
    /// </remarks>
    [MinLength(1, ErrorMessage = "There must be at least one suspicious string")]
    required public string[] Suspicious
    {
        get => _suspicious;
        init
        {
            _suspicious = value
                .Select(VerdictCalculationOptions.NormalizeAlertString)
                .ToArray();
        }
    }

    /// <summary>
    /// Gets the normalized undetected verdicts strings that sensors could return.
    /// </summary>
    /// <remarks>
    /// The verdict strings are normalized by removing hyphens, underscores, spaces, and trimming whitespace.
    /// Converts the string to lowercase.
    /// </remarks>
    [MinLength(1, ErrorMessage = "There must be at least one undetected string")]
    required public string[] Undetected
    {
        get => _undetected;
        init
        {
            _undetected = value
                .Select(VerdictCalculationOptions.NormalizeAlertString)
                .ToArray();
        }
    }
}
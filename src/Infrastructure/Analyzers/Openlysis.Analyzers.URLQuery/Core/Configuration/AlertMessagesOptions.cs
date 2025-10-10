using System.ComponentModel.DataAnnotations;

namespace Openlysis.Analyzers.URLQuery.Core.Configuration;

/// <summary>
/// Represents the options for alert messages.
/// </summary>
public record AlertMessagesOptions
{
    private readonly string[] _malicious = [];
    private readonly string[] _suspicious = [];

    /// <summary>
    /// Gets the normalized malicious alert message strings that sensors could return.
    /// </summary>
    /// <remarks>
    /// The alert message strings are normalized by removing hyphens, underscores, spaces, and trimming whitespace.
    /// Converts the string to lowercase.
    /// </remarks>
    [MinLength(1, ErrorMessage = "There must be at least one malicious alert message string")]
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
    /// Gets the normalized suspicious alert message strings that sensors could return.
    /// </summary>
    /// <remarks>
    /// The alert message strings are normalized by removing hyphens, underscores, spaces, and trimming whitespace.
    /// Converts the string to lowercase.
    /// </remarks>
    [MinLength(1, ErrorMessage = "There must be at least one suspicious alert message string")]
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
}
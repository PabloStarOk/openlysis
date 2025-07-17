namespace Openlysis.Domain.Common.ValueObjects;

/// <summary>
/// A set of IDs given by an external service to an analysis.
/// </summary>
public record ExternalAnalysisId
{
    private const char CharSeparator = ':';
    private readonly string? _job;

    /// <summary>
    /// Gets the primary ID given by the external service.
    /// </summary>
    public string Primary { get; }

    /// <summary>
    /// Gets the ID of the job associated with the analysis.
    /// </summary>
    public string? Job
    {
        get => _job;
        private init
        {
            _job = string.IsNullOrWhiteSpace(value) ? null : value;
        }
    }

    private ExternalAnalysisId(string primary, string? job = null)
    {
        Primary = primary;
        Job = job;
    }

    /// <summary>
    /// Creates a new instance of <see cref="ExternalAnalysisId"/> with the specified primary ID and optional job ID.
    /// </summary>
    /// <param name="primary">The primary ID given by the external service.</param>
    /// <param name="jobId">The optional job ID associated with the analysis.</param>
    /// <returns>A new <see cref="ExternalAnalysisId"/> instance.</returns>
    public static ExternalAnalysisId Create(
        string primary,
        string? jobId = null)
    {
        return new ExternalAnalysisId(primary, jobId);
    }

    /// <summary>
    /// Parses the specified input string into an <see cref="ExternalAnalysisId"/>.
    /// The input should be in the format "Primary" or "Primary:Job".
    /// </summary>
    /// <param name="input">The input string to parse.</param>
    /// <returns>An <see cref="ExternalAnalysisId"/> instance parsed from the input.</returns>
    /// <exception cref="InvalidOperationException">
    /// Thrown if the input contains more than two values separated by the character separator.
    /// </exception>
    public static ExternalAnalysisId Parse(string input)
    {
        string[] values = input.Split(CharSeparator);
        if (values.Length > 2)
        {
            throw new InvalidOperationException($"External analysis ID contains more than two values separated by {CharSeparator}");
        }

        string? jobId = values.Length is 2 ? values[1] : null;

        return Create(values[0], jobId);
    }

    /// <inheritdoc/>
    public override string ToString()
    {
        return Job is null
            ? Primary
            : $"{Primary}{CharSeparator}{Job}";
    }
}
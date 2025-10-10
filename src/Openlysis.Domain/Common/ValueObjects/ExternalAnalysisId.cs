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
    /// Gets the name of the external service that provided the analysis ID.
    /// </summary>
    public string Service { get; }

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

    private ExternalAnalysisId(string primary, string service, string? job = null)
    {
        Primary = primary;
        Service = service;
        Job = job;
    }

    /// <summary>
    /// Creates a new instance of <see cref="ExternalAnalysisId"/> with the specified primary ID, service, and optional job ID.
    /// </summary>
    /// <param name="primary">The primary ID given by the external service.</param>
    /// <param name="service">The name of the external service that provided the analysis ID.</param>
    /// <param name="jobId">The optional job ID associated with the analysis.</param>
    /// <returns>A new <see cref="ExternalAnalysisId"/> instance.</returns>
    public static ExternalAnalysisId Create(
        string primary,
        string service,
        string? jobId = null)
    {
        return new ExternalAnalysisId(primary, service, jobId);
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
        if (values.Length > 3)
        {
            throw new InvalidOperationException($"External analysis ID contains more than three values separated by {CharSeparator}");
        }

        string primary = values[0];
        string? jobId = values.Length is 3 ? values[1] : null;
        string service = values[^1];

        return Create(primary, service, jobId);
    }

    /// <inheritdoc/>
    public override string ToString()
    {
        return Job is null
            ? $"{Primary}{CharSeparator}{Service}"
            : $"{Primary}{CharSeparator}{Job}{CharSeparator}{Service}";
    }
}
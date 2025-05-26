namespace Openlysis.Domain.Common.ValueObjects;

/// <summary>
/// Represents a normalized threat score value object.
/// </summary>
public sealed record ThreatScore
{
    /// <summary>
    /// Gets the normalized threat score value, or null if not available.
    /// </summary>
    public int? NormalizedValue { get; }

    /// <summary>
    /// Gets the raw (unnormalized) threat score value, or null if not available.
    /// </summary>
    public float? RawValue { get; }

    /// <summary>
    /// Gets the maximum possible raw value for the threat score, or null if not available.
    /// </summary>
    public float? MaxPossibleRawValue { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="ThreatScore"/> class.
    /// </summary>
    /// <param name="normalizedValue">The normalized threat score value, or null if not available.</param>
    /// <param name="rawValue">The raw (unnormalized) threat score value, or null if not available.</param>
    /// <param name="maxPossibleValue">The maximum possible value for the threat score, or null if not available.</param>
    private ThreatScore(
        int? normalizedValue,
        float? rawValue,
        float? maxPossibleValue)
    {
        NormalizedValue = normalizedValue;
        RawValue = rawValue;
        MaxPossibleRawValue = maxPossibleValue;
    }

#pragma warning disable CS8618
#pragma warning disable S1144
    /// <summary>
    /// Initializes a new instance of the <see cref="ThreatScore"/> class,
    /// normalizing the provided raw value using the given maximum possible raw value.
    /// </summary>
    /// <param name="rawValue">The raw (unnormalized) threat score value, or null if not available.</param>
    /// <param name="maxPossibleRawValue">The maximum possible raw value for the threat score, or null if not available.</param>
    private ThreatScore(float? rawValue, float? maxPossibleRawValue)
    {
        NormalizedValue = NormalizeThreatScore(rawValue, maxPossibleRawValue);
        RawValue = rawValue;
        MaxPossibleRawValue = maxPossibleRawValue;
    }
#pragma warning restore S1144
#pragma warning restore CS8618

    /// <summary>
    /// Creates a new <see cref="ThreatScore"/> instance by normalizing the given score
    /// based on the provided maximum possible score.
    /// </summary>
    /// <param name="rawScore">The threat score to normalize.</param>
    /// <param name="maxPossibleScore">The maximum possible score for normalization.</param>
    /// <returns>A new <see cref="ThreatScore"/> instance with the normalized value.</returns>
    public static ThreatScore Create(float? rawScore, float? maxPossibleScore)
    {
        int? normalizedValue = NormalizeThreatScore(rawScore, maxPossibleScore);
        return new ThreatScore(
            normalizedValue,
            rawScore,
            maxPossibleScore);
    }

    /// <summary>
    /// Creates a <see cref="ThreatScore"/> instance representing a null or unavailable score.
    /// </summary>
    /// <returns>A <see cref="ThreatScore"/> instance with a null value.</returns>
    internal static ThreatScore CreateNull()
    {
        return new ThreatScore(null, null, null);
    }

    /// <summary>
    /// Normalizes a threat score to an integer value between 0 and 100.
    /// Returns null if the threat score is null.
    /// Throws <see cref="ArgumentException"/> if a score is provided but the max possible score is null.
    /// </summary>
    /// <param name="threatScore">The threat score to normalize.</param>
    /// <param name="maxPossibleScore">The maximum possible score for normalization.</param>
    /// <returns>The normalized threat score as an integer between 0 and 100, or null if the input score is null.</returns>
    private static int? NormalizeThreatScore(
        float? threatScore,
        float? maxPossibleScore)
    {
        if (threatScore is not { } nonNullScore)
        {
            return null;
        }

        if (maxPossibleScore is not { } nonNullMaxScore)
        {
            throw new ArgumentException(
                "A threat score was given, but the given max possible score is null.",
                paramName: nameof(maxPossibleScore));
        }

        nonNullScore /= nonNullMaxScore;
        float clampedValue = Math.Clamp(nonNullScore, 0.0f, 1.0f);
        double roundedValue = Math.Round(clampedValue * 100);
        return (int)roundedValue;
    }
}
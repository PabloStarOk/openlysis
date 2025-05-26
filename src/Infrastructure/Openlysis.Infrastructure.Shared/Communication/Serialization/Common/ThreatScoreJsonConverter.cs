using System.Text.Json;
using System.Text.Json.Serialization;

using Openlysis.Domain.Common.ValueObjects;

namespace Openlysis.Infrastructure.Shared.Communication.Serialization.Common;

/// <summary>
/// A custom <see cref="JsonConverter{T}"/> for serializing and deserializing <see cref="ThreatScore"/> objects.
/// </summary>
internal class ThreatScoreJsonConverter : JsonConverter<ThreatScore>
{
    private const string ObjectKey = nameof(ThreatScore);
    private const string RawValueKey = nameof(ThreatScore.NormalizedValue);
    private const string MaxPossibleRawValueKey = nameof(ThreatScore.MaxPossibleRawValue);

    /// <inheritdoc/>
    public override ThreatScore Read(
        ref Utf8JsonReader reader,
        Type typeToConvert,
        JsonSerializerOptions options)
    {
        StringComparison strComparison = options.PropertyNameCaseInsensitive
            ? StringComparison.OrdinalIgnoreCase
            : StringComparison.Ordinal;
        string rawValueKey = options.PropertyNamingPolicy?.ConvertName(RawValueKey)
            ?? RawValueKey;
        string maxValueKey = options.PropertyNamingPolicy?.ConvertName(MaxPossibleRawValueKey)
            ?? MaxPossibleRawValueKey;

        float? rawValue = null;
        float? maxPossibleRawValue = null;
        while (reader.Read())
        {
            if (reader.TokenType is JsonTokenType.StartObject)
            {
                continue;
            }

            if (reader.TokenType is JsonTokenType.EndObject)
            {
                break;
            }

            string? key = reader.GetString();
            reader.Read();
            if (key is not null
                && key.Equals(rawValueKey, strComparison)
                && reader.TokenType is JsonTokenType.Number)
            {
                rawValue = (float)reader.GetDecimal();
            }

            if (key is not null
                && key.Equals(maxValueKey, strComparison)
                && reader.TokenType is JsonTokenType.Number)
            {
                maxPossibleRawValue = (float)reader.GetDecimal();
            }
        }

        return ThreatScore.Create(rawValue, maxPossibleRawValue);
    }

    /// <inheritdoc/>
    public override void Write(
        Utf8JsonWriter writer,
        ThreatScore value,
        JsonSerializerOptions options)
    {
        string objectKey = options.PropertyNamingPolicy?.ConvertName(ObjectKey)
            ?? ObjectKey;
        string rawValueKey = options.PropertyNamingPolicy?.ConvertName(RawValueKey)
            ?? RawValueKey;
        string maxValueKey = options.PropertyNamingPolicy?.ConvertName(MaxPossibleRawValueKey)
            ?? MaxPossibleRawValueKey;

        writer.WriteStartObject(objectKey);
        WriteFloatOrNull(writer, rawValueKey, value.RawValue);
        WriteFloatOrNull(writer, maxValueKey, value.MaxPossibleRawValue);
        writer.WriteEndObject();
    }

    /// <summary>
    /// Writes a float value as a JSON number if not null; otherwise, writes a JSON null for the specified property name.
    /// </summary>
    /// <param name="writer">The <see cref="Utf8JsonWriter"/> to write to.</param>
    /// <param name="propertyName">The name of the property to write.</param>
    /// <param name="value">The nullable float value to write.</param>
    private static void WriteFloatOrNull(
        Utf8JsonWriter writer,
        string propertyName,
        float? value)
    {
        if (value is not null)
        {
            writer.WriteNumber(propertyName, (decimal)value);
        }
        else
        {
            writer.WriteNull(propertyName);
        }
    }
}
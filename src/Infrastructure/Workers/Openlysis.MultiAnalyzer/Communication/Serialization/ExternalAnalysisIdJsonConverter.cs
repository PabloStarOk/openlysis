using System;
using System.Text.Json;
using System.Text.Json.Serialization;

using Openlysis.Domain.Common.ValueObjects;

namespace Openlysis.MultiAnalyzer.Communication.Serialization;

/// <summary>
/// A custom <see cref="JsonConverter{T}"/> for serializing and deserializing <see cref="ExternalAnalysisId"/> objects.
/// </summary>
internal sealed class ExternalAnalysisIdJsonConverter : JsonConverter<ExternalAnalysisId>
{
    /// <inheritdoc/>
    public override ExternalAnalysisId Read(
        ref Utf8JsonReader reader,
        Type typeToConvert,
        JsonSerializerOptions options)
    {
        if (reader.TokenType == JsonTokenType.Null)
        {
            return null;
        }

        if (reader.TokenType == JsonTokenType.String)
        {
            var value = reader.GetString();
            return value is null ? null : ExternalAnalysisId.Parse(value);
        }

        throw new JsonException($"Unexpected token type {reader.TokenType} when parsing {nameof(ExternalAnalysisId)}.");
    }

    /// <inheritdoc/>
    public override void Write(Utf8JsonWriter writer, ExternalAnalysisId value, JsonSerializerOptions options)
    {
        writer.WriteStringValue(value.ToString());
    }
}
using System.Text.Json;
using System.Text.Json.Serialization;

using Openlysis.Domain.Common.ValueObjects;

namespace Openlysis.Infrastructure.Shared.Messaging.Serialization.Common;

/// <summary>
/// Converts a <see cref="GlobalId"/> object to and from JSON.
/// </summary>
internal class GlobalIdConverter : JsonConverter<GlobalId>
{
    private const string IdKey = "id";

    /// <inheritdoc/>
    public override GlobalId Read(
        ref Utf8JsonReader reader,
        Type typeToConvert,
        JsonSerializerOptions options)
    {
        // Read start object.
        reader.Read();

        // Read ID.
        reader.Read();
        var id = reader.GetString();
        ArgumentException.ThrowIfNullOrWhiteSpace(id);

        // Create ID object.
        Guid guid = Guid.Parse(id);
        var globalId = GlobalId.Parse(guid);

        // Read end of object
        reader.Read();
        return globalId;
    }

    /// <inheritdoc/>
    public override void Write(
        Utf8JsonWriter writer,
        GlobalId value,
        JsonSerializerOptions options)
    {
        string idKey = options.PropertyNamingPolicy?.ConvertName(IdKey) ?? IdKey;

        writer.WriteStartObject();
        writer.WriteString(idKey, value.Value);
        writer.WriteEndObject();
    }
}
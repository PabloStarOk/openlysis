using MessagePack;
using MessagePack.Formatters;

namespace Openlysis.API.Serialization;

/// <summary>
/// MessagePack formatter for serializing and deserializing <see cref="DateTime"/> values as Unix epoch milliseconds.
/// </summary>
internal sealed class DateTimeUnixEpochFormatter : IMessagePackFormatter<DateTimeOffset>
{
    /// <inheritdoc/>
    public void Serialize(ref MessagePackWriter writer, DateTimeOffset value, MessagePackSerializerOptions options)
    {
        writer.WriteInt64(value.ToUnixTimeMilliseconds());
    }

    /// <inheritdoc/>
    public DateTimeOffset Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options)
    {
        long milliseconds = reader.ReadInt64();
        return DateTimeOffset.FromUnixTimeMilliseconds(milliseconds);
    }
}
using System.Text.Json;
using System.Text.Json.Serialization;
using Ink.SourceGenerator.Packet;

namespace Ink.SourceGenerator.Converters.Json;

public sealed class PacketFieldTypeJsonConverter : JsonConverter<IPacketFieldType>
{
    public override IPacketFieldType? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        => IPacketFieldType.Parse(reader.GetString()!);

    public override void Write(Utf8JsonWriter writer, IPacketFieldType value, JsonSerializerOptions options)
    {
        throw new NotImplementedException();
    }
}

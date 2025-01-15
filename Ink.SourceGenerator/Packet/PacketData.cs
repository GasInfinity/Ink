using System.Text.Json.Serialization;

namespace Ink.SourceGenerator.Packet;

[method: JsonConstructor]
public readonly record struct PacketData(string? Name, OrderedDictionary<string, IPacketFieldType>? Fields)
{
    [JsonPropertyName("name")]
    public readonly string? Name = Name;
    [JsonPropertyName("fields")]
    public readonly OrderedDictionary<string, IPacketFieldType>? Fields = Fields;
}

using System.Text.Json.Serialization;

namespace Ink.SourceGenerator.Packet;

[method: JsonConstructor]
public readonly record struct PacketDefinition(int ProtocolId)
{
    [JsonPropertyName("protocol_id")]
    public readonly int ProtocolId = ProtocolId;
}

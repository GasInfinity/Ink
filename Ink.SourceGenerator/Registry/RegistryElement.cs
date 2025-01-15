using System.Text.Json.Serialization;

namespace Ink.SourceGenerator.Registry;

public readonly record struct RegistryElement(int ProtocolId)
{
    [JsonPropertyName("protocol_id")]
    public readonly int ProtocolId = ProtocolId;
}

using System.Text.Json.Serialization;

namespace Ink.SourceGenerator.Registry;

[method: JsonConstructor]
public readonly record struct RegistryData(Dictionary<string, RegistryElement> Entries, int ProtocolId)
{
    [JsonPropertyName("entries")]
    public readonly Dictionary<string, RegistryElement> Entries = Entries;
    [JsonPropertyName("protocol_id")]
    public readonly int ProtocolId = ProtocolId;
}

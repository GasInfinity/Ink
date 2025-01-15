using System.Collections.Immutable;
using System.Runtime.CompilerServices;
using System.Text.Json.Serialization;

namespace Ink.SourceGenerator.Block;

[method: JsonConstructor]
public readonly record struct DefinedBlockState(ImmutableDictionary<string, string> Properties, int Id, bool IsDefault)
{   
    [JsonInclude]
    [JsonPropertyName("properties")]
    public readonly ImmutableDictionary<string, string> Properties = Properties;
    
    [JsonIgnore]
    private readonly int Raw = Id | (Unsafe.BitCast<bool, byte>(IsDefault) << 31);

    [JsonInclude]
    [JsonPropertyName("id")]
    public readonly int Id
        => (Raw & ~(1 << 31));

    [JsonInclude]
    [JsonPropertyName("default")]
    public readonly bool IsDefault
        => (Raw >>> 31) != 0;
}

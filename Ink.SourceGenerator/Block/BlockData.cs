using System.Collections.Immutable;
using System.Text.Json.Serialization;

namespace Ink.SourceGenerator.Block;

[method: JsonConstructor]
public readonly record struct BlockData(ImmutableDictionary<string, ImmutableArray<string>> Properties, ImmutableArray<DefinedBlockState> States)
{
    [JsonInclude]
    [JsonPropertyName("properties")]
    public readonly ImmutableDictionary<string, ImmutableArray<string>> Properties = Properties;

    [JsonInclude]
    [JsonPropertyName("states")]
    public readonly ImmutableArray<DefinedBlockState> States = States;
}

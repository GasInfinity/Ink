using System.Collections.Immutable;

namespace Ink.SourceGenerator.Block;

public readonly record struct Blocks(ImmutableDictionary<string, BlockData> Data)
{
    public readonly ImmutableDictionary<string, BlockData> Data = Data;
}

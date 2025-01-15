using Ink.Chunks;
using Ink.Registries;
using System.Collections.Immutable;

namespace Ink.Blocks.State;

public readonly record struct BlockStateChild(BlockStateRoot Root, ImmutableArray<Property> Properties, int Id, int UniqueRootIndex)
{
    public readonly BlockStateRoot Root = Root;
    public readonly ImmutableArray<Property> Properties = Properties;
    public readonly int Id = Id;
    public readonly int UniqueRootIndex = UniqueRootIndex;

    public BlockStateChild(BlockStateRoot root, ImmutableArray<PropertyDefinition> properties, BlockStateDefinition definition) : this(root, definition.Properties, definition.State, definition.ComputeUniqueIndex(properties))
    {
    }

    public bool TryWithProperty<T>(string property, T value, out BlockStateChild state)
        where T : unmanaged
        => Root.TryWithProperty(this, property, value, out state);

    public BlockStateChild WithProperty<T>(string property, T value)
        where T : unmanaged
        => TryWithProperty(property, value, out BlockStateChild state) ? state : this;

    public bool TryGetProperty(string property, out int value)
    {
        if(!Root.PropertyKeyIndex.TryGetValue(property, out int index))
        {
            value = default;
            return false;
        }

        value = Properties.ItemRef(index).Value;
        return true;
    }

    public int GetProperty(string property)
        => TryGetProperty(property, out int value) ? value : default;

    public Block? GetBlock(FrozenRegistry<Block> block)
        => Root.GetBlock(block);

    public static implicit operator StateStorage(in BlockStateChild state)
        => state.Id;
}

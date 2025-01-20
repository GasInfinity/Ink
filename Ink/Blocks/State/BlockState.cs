using Ink.Chunks;
using Ink.Registries;

namespace Ink.Blocks.State;

public readonly record struct BlockState(BlockStateRoot Root, int Id, int RootIndex)
{
    public readonly BlockStateRoot Root = Root;
    public readonly int Id = Id;
    public readonly int RootIndex = RootIndex;

    public bool TryWithProperty<TValue>(string property, TValue value, out BlockState state)
        where TValue : unmanaged
        => Root.TryWithProperty(RootIndex, property, value, out state);

    public BlockState WithProperty<TValue>(string property, TValue value)
        where TValue : unmanaged
        => TryWithProperty(property, value, out BlockState state) ? state : this;

    public bool TryGetProperty<TValue>(string name, out TValue value)
        where TValue : unmanaged
    {
        if(!Root.PropertyKeyIndex.TryGetValue(name, out int index))
        {
            value = default;
            return false;
        }

        (Property property, byte bitsUsed, byte bitOffset) = Root.Properties[index];
        int propertyValueIndex = (RootIndex >> bitOffset) & ((1 << bitsUsed) - 1);

        return property.TryGetValue(propertyValueIndex, out value);
    }

    public TValue GetProperty<TValue>(string name)
        where TValue : unmanaged
        => TryGetProperty(name, out TValue value) ? value : default;

    public Block? GetBlock(FrozenRegistry<Block> block)
        => Root.GetBlock(block);

    public override string? ToString()
        =>  $"{Root.Location} [{RootIndex:B32} / {Id}]";

    public static implicit operator StateStorage(in BlockState state)
        => state.Id;
}

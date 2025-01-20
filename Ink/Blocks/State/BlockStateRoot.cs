using Ink.Registries;
using System.Collections.Frozen;
using System.Collections.Immutable;
using System.Runtime.CompilerServices;

namespace Ink.Blocks.State;

/*
 Design: I have designed this with performance in mind, this should be memory efficient and potentially very fast.

 Constructor(Dictionary<string, int> keyIndex, ImmutableArray<PropertyRange> properties, ImmutableArray<BlockStateDefinition> states, BlockStateDefinition defaultState)
 readonly record BlockStateRoot -> FrozenDictionary<string, int> PropertyKeyIndex, ImmutableArray<PropertyRange> Properties, FrozenDictionary<int, BlockStateChild> UniquePropertyState, BlockStateDefinition Default
   - TryWithProperty(string name, byte rawValue, out BlockStateChild state);
   - TryWithProperty<T>(BlockStateDefinition base, string name, T value, out BlockStateChild state);

 readonly record struct BlockStateChild -> BlockStateDefinitionRoot BlockStateRoot, ImmutableArray<Property> Properties, int State, int UniqueRootIndex
   - TryGetPropertyAs<T>(string name);
   - TryWithProperty<T>(string name, T value, out BlockStateDefinition definition);
 
 readonly record struct Property(Int32Range, EnumValues, Boolean) -> Has Different Types
 
 Block -> private readonly BlockStateDefinitionRoot
   - DefaultState -> BlockStateChild (From BlockStateRoot) -> .State -> BlockState (int)

    FrozenDictionary<int, BlockStateDefinition> States -> All
*/
public sealed record BlockStateRoot
{
    public readonly Identifier Location;
    public readonly FrozenDictionary<string, int> PropertyKeyIndex;
    public readonly ImmutableArray<(Property Definition, byte BitsUsed, byte BitOffset)> Properties;
    public readonly FrozenDictionary<int, int> StateCombinations;
    public readonly int DefaultIndex;

    public BlockState Default
        => new(this, StateCombinations[DefaultIndex], DefaultIndex);

    public BlockStateRoot(Identifier identifier, FrozenDictionary<string, int> keyIndex, ImmutableArray<(Property, byte, byte)> properties, FrozenDictionary<int, int> stateDefinitions, int defaultIndex)
    {
        Location = identifier;
        PropertyKeyIndex = keyIndex;
        Properties = properties;
        StateCombinations = stateDefinitions;
        DefaultIndex = defaultIndex;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool TryGetProperty(string property, out (Property Property, int BitsUsed, byte BitOffset) result)
    {
        if(!PropertyKeyIndex.TryGetValue(property, out int index))
        {
            result = default;
            return false;
        }

        result = Properties[index];
        return true;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool TryWithProperty<T>(int rootIndex, string name, T value, out BlockState state)
        where T : unmanaged
    {
        if (!PropertyKeyIndex.TryGetValue(name, out int nameIndex))
        {
            state = default;
            return false;
        }

        (Property property, int bitsUsed, int bitOffset) = Properties[nameIndex];
        if (!property.TryGetIndex(value, out int propertyIndex))
        {
            state = default;
            return false;
        }

        int maxPropertyValue = (1 << bitsUsed) - 1;
        int bitsMask = ~(maxPropertyValue << bitOffset);
        int newUniqueIndex = (rootIndex & bitsMask) | (propertyIndex << bitOffset);

        state = new(this, StateCombinations[newUniqueIndex], newUniqueIndex); // Will never be null, we don't need TryGet here
        return true;
    }

    public Block? GetBlock(FrozenRegistry<Block> registry)
        => registry.Get(Location);
}

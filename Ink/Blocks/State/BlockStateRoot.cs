using Ink.Registries;
using Ink.Util;
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
 
 // I don't think mojang will allow having more than 8 bits for a property (2^8 -> 256 states for only 1 property! add other bit and you'll have 512, that won't happen)
 readonly record struct Property -> byte Data [Value] (8 bits for value)
 readonly record struct PropertyRange -> byte Data [(PropertyType << 4) | BitsNeeded] (Same)
 
 Block -> private readonly BlockStateDefinitionRoot
   - DefaultState -> BlockStateChild (From BlockStateRoot) -> .State -> BlockState (int)

    FrozenDictionary<int, BlockStateDefinition> States -> All
*/
public record BlockStateRoot
{
    public readonly Identifier Identifier;
    public readonly FrozenDictionary<string, int> PropertyKeyIndex;
    public readonly ImmutableArray<PropertyDefinition> Properties;
    public readonly FrozenDictionary<int, BlockStateChild> UniquePropertyState;
    public readonly int DefaultRootIndex;

    public ref readonly BlockStateChild Default
        => ref UniquePropertyState.GetValueRefOrNullRef(DefaultRootIndex);

    public BlockStateRoot(Identifier identifier, FrozenDictionary<string, int> keyIndex, ImmutableArray<PropertyDefinition> properties, params ReadOnlySpan<BlockStateDefinition> stateDefinitions)
    {
        Identifier = identifier;
        PropertyKeyIndex = keyIndex;
        Properties = properties;

        Dictionary<int, BlockStateChild> uniquePropertyState = new (stateDefinitions.Length);
        foreach(var state in stateDefinitions)
        {
            BlockStateChild child = new(this, properties, state);

            if(uniquePropertyState.TryGetValue(child.UniqueRootIndex, out BlockStateChild c))
            {
                Console.WriteLine($"DUPLICATE: {c.Id} {child.Id}");
            }
            uniquePropertyState.Add(child.UniqueRootIndex, child);

            if (state.IsDefault)
                DefaultRootIndex = child.UniqueRootIndex;
        }

        UniquePropertyState = uniquePropertyState.ToFrozenDictionary();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool TryGetPropertyDefinition(string property, out PropertyDefinition result)
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
    public bool TryWithProperty<T>(in BlockStateChild baseChild, string property, T value, out BlockStateChild state)
        where T : unmanaged
    {
        if (!PropertyKeyIndex.TryGetValue(property, out int index))
        {
            state = default;
            return false;
        }

        PropertyDefinition propertyDefinition = Properties[index];
        if (!propertyDefinition.Kind.IsCompatible<T>())
        {
            state = default;
            return false;
        }

        if (!Utilities.TryGetInteger(value, out ulong result))
        {
            state = default;
            return false;
        }

        int max = propertyDefinition.Max;
        if(result > (ulong)max)
        {
            state = default;
            return false;
        }

        int shift = default;

        for(int i = 0; i < index; ++i)
            shift += Properties[i].BitsUsed;

        int bitsMask = ~(propertyDefinition.Max << shift);
        int newUniqueIndex = (baseChild.UniqueRootIndex & bitsMask) | (int)((result - (ulong)propertyDefinition.Offset) << shift);

        state = UniquePropertyState.GetValueRefOrNullRef(newUniqueIndex); // Will never be null, we don't need TryGet here
        return true;
    }

    public Block? GetBlock(FrozenRegistry<Block> registry)
        => registry.Get(Identifier);
}

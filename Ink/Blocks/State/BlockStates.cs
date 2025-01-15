using System.Collections.Frozen;

namespace Ink.Blocks.State;

public static partial class BlockStates
{
    // All these things are initialized by the source generated file!
    private static readonly FrozenDictionary<int, (BlockStateRoot, int)> AllStates = FrozenDictionary<int, (BlockStateRoot, int)>.Empty;
    public static readonly int StateCount;
    public static readonly byte MaxStateBits;

    public static bool TryGetState(int id, out BlockStateChild state)
    {
        if (!AllStates.TryGetValue(id, out (BlockStateRoot, int) stateData))
        {
            state = default;
            return false;
        }

        state = stateData.Item1.UniquePropertyState.GetValueRefOrNullRef(stateData.Item2);
        return true;
    }

    public static BlockStateChild GetState(int id)
    {
        if(TryGetState(id, out BlockStateChild state))
            return state;

        Console.WriteLine($"Bro??? {id}");
        return BlockStates.Air.Root.Default;
    }

    private static void AddRoot(Dictionary<int, (BlockStateRoot, int)> values, BlockStateRoot root)
    {
        foreach(int key in root.UniquePropertyState.Keys)
        {
            ref readonly BlockStateChild child = ref root.UniquePropertyState.GetValueRefOrNullRef(key);
            values.Add(child.Id, (child.Root, child.UniqueRootIndex));
        }
    }
}

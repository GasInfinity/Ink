using System.Collections.Immutable;
using System.Diagnostics;

namespace Ink.Blocks.State;

public static partial class BlockStates
{
    // All these things are initialized by the source generated file!
    private static readonly ImmutableArray<(BlockStateRoot, int)> AllStates = ImmutableArray<(BlockStateRoot, int)>.Empty;
    // public const int StateCount; SourceGen
    // public const byte MaxStateBits; SourceGen

    public static bool TryGetState(int id, out BlockState state)
    {
        if (id >= StateCount)
        {
            state = default;
            return false;
        }

        (BlockStateRoot Root, int RootIndex) = AllStates[id];
        state = new (Root, id, RootIndex);
        return true;
    }

    public static BlockState GetState(int id)
        => TryGetState(id, out BlockState state) ? state : throw new UnreachableException($"Unknown id {id}");

    private static void AddRoot(ImmutableArray<(BlockStateRoot, int)>.Builder values, BlockStateRoot root)
    {
        foreach(int key in root.StateCombinations.Keys)
        {
            int id = root.StateCombinations.GetValueRefOrNullRef(key);
            values[id] = ((root, key));
        }
    }
}

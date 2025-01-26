using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Ink.Blocks.State;
using Ink.Chunks;
using Ink.Math;
using Ink.Worlds;

namespace Ink.Server.Worlds;

// TODO: World generation and chunk loading from disk
public sealed class ServerChunkManager(DimensionType CachedDimensionType) : IWorldChunkManager
{
    // TODO: Chunk ticket system
    private readonly Dictionary<ChunkPosition, Chunk> loadedChunks = new();
    private readonly DimensionType CachedDimensionType = CachedDimensionType;

    public void Tick()
    {
    }

    public ref Chunk GetChunkRefOrNullRef(ChunkPosition position)
    {
        ref Chunk chunk = ref CollectionsMarshal.GetValueRefOrAddDefault(this.loadedChunks, position, out bool exists);

        if(!exists)
        {
            chunk = new (CachedDimensionType.MinY, CachedDimensionType.Height);

            for(int x = 0; x < 16; ++x)
            {
                for(int z = 0; z < 16; ++z)
                {
                    chunk.SetBlockState(CachedDimensionType.MinY, x, 0, z, BlockStates.Stone.Root.Default);
                }
            }
        }

        return ref chunk; 
    }

    public bool IsCurrentlyLoaded(ChunkPosition position)
        => this.loadedChunks.ContainsKey(position); 

    public ValueTask DisposeAsync()
    {
        return ValueTask.CompletedTask;
    }
}

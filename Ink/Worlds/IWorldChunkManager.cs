using Ink.Chunks;
using Ink.Math;

namespace Ink.Worlds;

public interface IWorldChunkManager : ITickable, IAsyncDisposable
{
    ref Chunk GetChunkRefOrNullRef(ChunkPosition position);

    bool IsCurrentlyLoaded(ChunkPosition position);
}

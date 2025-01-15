using Ink.Nbt;
using Ink.Util;

namespace Ink.Chunks;

public static class Heightmaps
{
    // TODO: Do not hardcode
    public static PooledCompactedData WriteMotionBlocking(ref Chunk chunk)
    {
        byte needed = Utilities.BitSize(chunk.Height + 1);
        PooledCompactedData data = new(Chunk.HorizontalSurface, needed);

        for(int x = 0; x < Chunk.HorizontalSize; ++x)
        {
            for(int z = 0; z < Chunk.HorizontalSize; ++z)
            {
                data[x + z * Chunk.HorizontalSize] = 0;
            }
        }

        return data;
    }
}

using Rena.Mathematics;

namespace Ink.Math;

public readonly record struct ChunkPosition(Vec2<int> Vec)
{
    public readonly Vec2<int> Vec = Vec;

    public readonly int X
        => Vec.X;
    public readonly int Z
        => Vec.Y;

    public ChunkPosition(int x, int z) : this(new(x, z))
    {
    }

    public ChunkPosition Relative(ChunkPosition pos)
        => new(Vec + pos.Vec);
    
    public ChunkPosition Relative(int x, int z)
        => new(Vec + new Vec2<int>(x, z));
}

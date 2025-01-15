using Rena.Mathematics;

namespace Ink.Math;

public readonly record struct RegionPosition(Vec2<int> Vec)
{
    public readonly Vec2<int> Vec = Vec;

    public readonly int X
        => Vec.X;
    public readonly int Z
        => Vec.Y;

    public RegionPosition(int x, int z) : this(new(x, z))
    {
    }

    public RegionPosition Relative(RegionPosition pos)
        => Relative(pos.Vec);
    
    public RegionPosition Relative(int x, int z)
        => Relative(new Vec2<int>(x, z));
    
    public RegionPosition Relative(Vec2<int> position)
        => new(Vec + position);
}
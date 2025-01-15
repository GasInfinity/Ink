using System.Collections;
using Ink.Math;
using Rena.Mathematics;

namespace Ink.World;

public struct BlockPositionEnumerable
{
    public readonly Vec3<int> StartPosition;
    public readonly Vec3<int> Size;
    public readonly int Length;
    private int currentIndex;
    private BlockPosition current;

    public BlockPositionEnumerable(Vec3<int> min, Vec3<int> max)
    {
        StartPosition = min;
        Size = (max - min) + new Vec3<int>(1, 1, 1);
        Length = Size.X * Size.Y * Size.Z;
    }

    public BlockPosition Current
        => current;

    public bool MoveNext()
    {
        if (currentIndex == Length)
            return false;

        (int extra, int x) = int.DivRem(currentIndex, Size.X);
        (int z, int y) = int.DivRem(extra, Size.Y);

        current = new(StartPosition + new Vec3<int>(x, y, z));
        ++currentIndex;
        return true;
    }

    public readonly BlockPositionEnumerable GetEnumerator()
        => this;

    public static BlockPositionEnumerable FromAabb(in Aabb aabb)
        => new(((BlockPosition)aabb.Min).Vec, ((BlockPosition)aabb.Max).Vec);
}
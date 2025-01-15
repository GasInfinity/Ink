using System.Runtime.CompilerServices;
using Rena.Mathematics;

namespace Ink.Math;

public readonly record struct Aabb(in Vec3<double> Min, in Vec3<double> Max)
{
    public static Aabb Empty => default;
    public static Aabb Cube => new(0, 0, 0, 1, 1, 1);

    public readonly Vec3<double> Min = Min;
    public readonly Vec3<double> Max = Max;

    public readonly double MinX
        => Min.X;
    public readonly double MinY
        => Min.Y;
    public readonly double MinZ
        => Min.Z;
    public readonly double MaxX
        => Max.X;
    public readonly double MaxY
        => Max.Y;
    public readonly double MaxZ
        => Max.Z;

    public Aabb(double MinX, double MinY, double MinZ, double MaxX, double MaxY, double MaxZ) : this(new(MinX, MinY, MinZ), new(MaxX, MaxY, MaxZ))
    {
    }

    public Aabb Relative(double x, double y, double z)
        => Relative(new Vec3<double>(x, y, z));

    public Aabb Relative(BlockPosition position)
        => Relative(position.X, position.Y, position.Z);

    public Aabb Relative(in Vec3<double> position)
        => new(Min + position, Max + position);

    public Aabb Expand(Vec3<double> value)
        => new(Min - value, Max + value);

    public Aabb Stretch(double x, double y, double z)
        => Stretch(new Vec3<double>(x, y, z));

    public Aabb Stretch(Vec3<double> value)
    {
        (double minX, double maxX) = value.X > 0 ? (MinX, MaxX + value.X) : (MinX + value.X, MaxX);
        (double minY, double maxY) = value.Y > 0 ? (MinY, MaxY + value.Y) : (MinY + value.Y, MaxY);
        (double minZ, double maxZ) = value.Z > 0 ? (MinZ, MaxZ + value.Z) : (MinZ + value.Z, MaxZ);

        return new(minX, minY, minZ, maxX, maxY, maxZ);
    }

    public bool Intersects(in Aabb other)
    {
        return MaxX > other.MinX && MinX < other.MaxX
            && MaxY > other.MinY && MinY < other.MaxY
            && MaxZ > other.MinZ && MinZ < other.MaxZ;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public double OverlapOffsetX(Aabb other, double movement)
        => movement > 0 ? MaxX - other.MinX : -(other.MaxX - MinX);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public double OverlapOffsetY(Aabb other, double movement)
        => movement > 0 ? MaxY - other.MinY : -(other.MaxY - MinY);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public double OverlapOffsetZ(Aabb other, double movement)
        => movement > 0 ? MaxZ - other.MinZ : -(other.MaxZ - MinZ);

    public static Aabb FromCenterCenterCenter(in Vec3<double> center, AabbDefinition definition)
        => new(center - definition.Half, center + definition.Half);

    public static Aabb FromCenterMinCenter(double centerX, double minY, double centerZ, AabbDefinition definition)
        => new(centerX - definition.HalfWidth, minY, centerZ - definition.HalfDepth,
               centerX + definition.HalfWidth, minY + definition.Height, centerZ + definition.HalfDepth);
}

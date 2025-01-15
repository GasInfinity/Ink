using System.Collections.Immutable;
using System.Runtime.CompilerServices;
using Ink.Util.Extensions;
using Rena.Mathematics;

namespace Ink.Math;

public abstract record Collider
{
    public static readonly Collider Empty = new EmptyCollider();
    public static readonly Collider Cube = new AxisAlignedBoundingBoxCollider(Aabb.Cube);

    public bool IsEmpty
        => ReferenceEquals(this, Empty);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool Intersects(Vec3<double> offset, in Aabb other)
    {
        if (IsEmpty)
            return false;

        if (this is AxisAlignedBoundingBoxCollider aabbCollider)
            return other.Intersects(aabbCollider.Aabb.Relative(offset));

        return false; // TODO: Composite
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public double OverlapOffsetX(Vec3<double> offset, in Aabb other, double movement)
    {
        if (IsEmpty)
            return 0;

        if (this is AxisAlignedBoundingBoxCollider aabbCollider)
            return other.OverlapOffsetX(aabbCollider.Aabb.Relative(offset), movement);

        return 0;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public double OverlapOffsetY(Vec3<double> offset, in Aabb other, double movement)
    {
        if (IsEmpty)
            return 0;

        if (this is AxisAlignedBoundingBoxCollider aabbCollider)
            return other.OverlapOffsetY(aabbCollider.Aabb.Relative(offset), movement);

        return 0;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public double OverlapOffsetZ(Vec3<double> offset, in Aabb other, double movement)
    {
        if (IsEmpty)
            return 0;

        if (this is AxisAlignedBoundingBoxCollider aabbCollider)
            return other.OverlapOffsetZ(aabbCollider.Aabb.Relative(offset), movement);

        return 0;
    }

    public static Collider FromAabb(in Aabb aabb)
        => new AxisAlignedBoundingBoxCollider(aabb);

    // TODO! We can join these methods maybe?
    public static double TotalAdjustedMovementX(ImmutableArray<(BlockPosition, Collider)> collisions, in Aabb other, double movement)
    {
        double lowestAdjustedMovement = movement;
        foreach ((BlockPosition position, Collider collision) in collisions)
        {
            Vec3<double> offset = Vec3<double>.CreateTruncating(position.Vec);

            if (!collision.Intersects(offset, other))
                continue;

            double overlap = collision.OverlapOffsetX(offset, other, movement);
            double adjustedMovement = movement - overlap;

            if (double.Abs(adjustedMovement) < double.Abs(lowestAdjustedMovement))
                lowestAdjustedMovement = adjustedMovement;

            if (lowestAdjustedMovement.AlmostEqual(0))
                return 0;
        }

        return lowestAdjustedMovement;
    }

    public static double TotalAdjustedMovementY(ImmutableArray<(BlockPosition, Collider)> collisions, in Aabb other, double movement)
    {
        double lowestAdjustedMovement = movement;
        foreach ((BlockPosition position, Collider collision) in collisions)
        {
            Vec3<double> offset = Vec3<double>.CreateTruncating(position.Vec);

            if (!collision.Intersects(offset, other))
                continue;

            double overlap = collision.OverlapOffsetY(offset, other, movement);
            double adjustedMovement = movement - overlap;

            if (double.Abs(adjustedMovement) < double.Abs(lowestAdjustedMovement))
                lowestAdjustedMovement = adjustedMovement;

            if (lowestAdjustedMovement.AlmostEqual(0))
                return 0;
        }

        return lowestAdjustedMovement;
    }

    public static double TotalAdjustedMovementZ(ImmutableArray<(BlockPosition, Collider)> collisions, in Aabb other, double movement)
    {
        double lowestAdjustedMovement = movement;
        foreach ((BlockPosition position, Collider collision) in collisions)
        {
            Vec3<double> offset = Vec3<double>.CreateTruncating(position.Vec);

            if (!collision.Intersects(offset, other))
                continue;

            double overlap = collision.OverlapOffsetZ(offset, other, movement);
            double adjustedMovement = movement - overlap;

            if (double.Abs(adjustedMovement) < double.Abs(lowestAdjustedMovement))
                lowestAdjustedMovement = adjustedMovement;

            if (lowestAdjustedMovement.AlmostEqual(0))
                return 0;
        }

        return lowestAdjustedMovement;
    }

    private sealed record EmptyCollider : Collider;
    private sealed record AxisAlignedBoundingBoxCollider(Aabb Aabb) : Collider;
}
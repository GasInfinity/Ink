using System.Collections.Immutable;
using Ink.Blocks;
using Ink.Blocks.State;
using Ink.Math;
using Rena.Mathematics;

namespace Ink.World;

public struct BlockCollisionEnumerable(BaseWorld world, in Aabb collider)
{
    public readonly BaseWorld World = world;
    public readonly Aabb CheckCollider = collider;
    private BlockPositionEnumerable positions = BlockPositionEnumerable.FromAabb(collider.Expand(new Vec3<double>(1, 1, 1)));
    private (BlockPosition, Collider) current = (default, Collider.Empty);

    public readonly (BlockPosition, Collider) Current
        => current;

    public bool MoveNext()
    {
        BaseWorld world = World;
        Aabb collider = CheckCollider;

        while(positions.MoveNext())
        {
            BlockPosition currentPosition = positions.Current;

            if(!world.IsCurrentlyLoaded(currentPosition))
                continue;

            BlockStateChild state = world.GetBlockState(currentPosition);

            if(state.Root == null)
            {
                Console.WriteLine($"WHAAAAAAAAT????? {state.Id} {currentPosition}");
                Console.WriteLine($"{state}");
            }
            Block? block = state.GetBlock(world.BlockRegistry);

            if(block == null)
                continue;

            Collider blockCollider = block.GetCollider(state, world, currentPosition);

            if(blockCollider.IsEmpty)
                continue;

            if(blockCollider.Intersects(Vec3<double>.CreateTruncating(currentPosition.Vec), collider))
            {
                current = (currentPosition, blockCollider);
                return true;
            }
        }

        return false;
    }

    public BlockCollisionEnumerable GetEnumerator()
        => this;

    public ImmutableArray<(BlockPosition, Collider)> ToImmutableArray()
    {
        ImmutableArray<(BlockPosition, Collider)>.Builder builder = ImmutableArray.CreateBuilder<(BlockPosition, Collider)>();

        foreach(var items in this)
            builder.Add(items);

        return builder.DrainToImmutable();
    }
}

using System.Runtime.CompilerServices;
using Friflo.Engine.ECS;
using Ink.Blocks;
using Ink.Blocks.State;
using Ink.Chunks;
using Ink.Math;
using Ink.Registries;
using Ink.Util;

namespace Ink.Worlds;

// FIXME: This can't be like this... Refactor this class ASAP
public abstract class World : ITickable, IAsyncDisposable
{
    public const int HorizontalSize = 30000000;
    public const int VerticalSize = 20000000;

    protected readonly IWorldChunkManager worldChunkManager;

    protected ThreadSafeFlag tickingFlag = new();
    protected long worldAge;

    public readonly WorldFlags Flags;
    public readonly Uuid Id;
    public readonly Identifier Dimension;
    public readonly DimensionType CachedDimensionType;
    public readonly FrozenRegistry<Block> BlockRegistry;

    public GameDifficulty Difficulty { get; set; }

    public bool IsClient
        => Flags.HasFlag(WorldFlags.IsClient);

    protected World(IWorldChunkManager chunkManager, WorldFlags flags, Uuid id, FrozenRegistry<Block> blockRegistry, FrozenRegistry<DimensionType> dimensionRegistry, Identifier dimension)
    {
        this.worldChunkManager = chunkManager;

        Flags = flags;
        Id = id;
        BlockRegistry = blockRegistry;
        Dimension = dimension;
        CachedDimensionType = dimensionRegistry.Get(dimension)
                            ?? throw new ArgumentException($"Dimension '{dimension}' not found inside dimensions registry");
    }

    public bool IsValid(BlockPosition position)
        => IsValidHorizontally(position.X, position.Z) && IsValidVertically(position.Y);

    public bool IsInBuildLimit(BlockPosition position)
        => IsValidHorizontally(position.X, position.Z) && IsInBuildLimit(position.Y);

    public bool IsValidHorizontally(int x, int z) // (x >= -HorizontalSize & x < HorizontalSize) & (z >= -HorizontalSize & z < HorizontalSize)
        => ((uint)(x + HorizontalSize) < (HorizontalSize * 2))
        && ((uint)(z + HorizontalSize) < (HorizontalSize * 2));

    public bool IsValidVertically(int y) // (y >= -VerticalSize & y < VerticalSize)
        => ((uint)(y + VerticalSize) < (VerticalSize * 2));

    public bool IsInBuildLimit(int y)
    {
        int minY = CachedDimensionType.MinY;
        int maxY = (CachedDimensionType.Height - minY);
        return y >= minY & y < maxY;
    }


    public virtual void SyncWorldEvent(Entity? entity, WorldEvent id, BlockPosition position, int data)
    {
    }

    public bool CanPlaceAt(BlockPosition position, in BlockState state, Block block, out Entity? possibleCollidingEntity)
    {
        if (!IsInBuildLimit(position))
        {
            possibleCollidingEntity = null;
            return false;
        }

        if (!(block?.BlockSettings.IsCollidable ?? true))
        {
            possibleCollidingEntity = null;
            return true;
        }

        //Collider blockCollider = (block?.GetCollider(state, World, absolute) ?? Collider.Cube).Relative(absolute);

        //if (blockCollider.IsEmpty)
        //{
        //    possibleCollidingEntity = null;
        //    return true;
        //}

        // foreach (BaseEntity entity in EntityManager.NearbyEntities(position))
        // {
        //     if (!entity.ObstructsBlockPlacements)
        //         continue;
        //
        //     Aabb entityCollider = entity.Box;
        //     if (false)//(blockCollider.Intersects(entityCollider))
        //     {
        //         possibleCollidingEntity = entity;
        //         return false;
        //     }
        // }

        possibleCollidingEntity = null;
        return true;
    }

    public bool BreakBlock(BlockPosition position, bool dropStacks, Entity? breakingEntity, int maxUpdateDepth)
    {
        BlockState lastState = GetBlockState(position);
        Block? block = lastState.GetBlock(BlockRegistry);

        // TODO: Fluid state
        bool changed = SetBlockState(position, BlockStates.Air.Root.Default, BlockStateChangeFlags.NotifyAll);

        if (changed)
            SyncWorldEvent(breakingEntity, WorldEvent.BlockBreak, position, lastState.Id);

        return changed;
    }

    public bool BreakBlock(BlockPosition position, bool dropStacks, Entity? breakingEntity)
        => BreakBlock(position, dropStacks, breakingEntity, 0); // TODO maxUpdateDepth

    public bool BreakBlock(BlockPosition position, bool dropStacks)
        => BreakBlock(position, dropStacks, null);

    public bool RemoveBlock(BlockPosition position, bool move)
    {
        BlockState lastState = GetBlockState(position);

        // TODO: Fluid state
        return SetBlockState(position, default, BlockStateChangeFlags.NotifyAll | (move ? BlockStateChangeFlags.Moved : 0));
    }

    public virtual bool SetBlockState(BlockPosition position, in BlockState state, BlockStateChangeFlags flags, int maxUpdateDepth)
    {
        int minY = CachedDimensionType.MinY;

        if (!IsInBuildLimit(position))
            return false;

        ChunkPosition chunkPosition = position.ToChunkPosition();
        int relativeX = Utilities.Modulo(position.X, Chunk.HorizontalSize);
        int relativeZ = Utilities.Modulo(position.Z, Chunk.HorizontalSize);

        ref Chunk chunk = ref worldChunkManager.GetChunkRefOrNullRef(chunkPosition);

        if (Unsafe.IsNullRef(ref chunk))
            return false;

        StateStorage lastStateId = chunk.SetBlockState(minY, relativeX, position.Y, relativeZ, state).RegistryId;

        if(state == lastStateId)
            return false;

        BlockState lastState = BlockStates.GetState(lastStateId.RegistryId);
        Block? block = lastState.GetBlock(BlockRegistry);

        block?.OnStateReplaced(lastState, this, position, state, flags.HasFlag(BlockStateChangeFlags.Moved));
        return true;
    }

    public bool SetBlockState(BlockPosition position, in BlockState state, BlockStateChangeFlags flags)
        => SetBlockState(position, state, flags, 0); // TODO: Default maxUpdateDepth

    public bool SetBlockState(BlockPosition position, in BlockState state)
        => SetBlockState(position, state, BlockStateChangeFlags.NotifyAll);

    public BlockState GetBlockState(BlockPosition position)
    {
        int minY = CachedDimensionType.MinY;

        if (!IsInBuildLimit(position))
            return BlockStates.VoidAir.Root.Default;

        ChunkPosition chunkPosition = position.ToChunkPosition();
        int relativeX = Utilities.Modulo(position.X, Chunk.HorizontalSize);
        int relativeZ = Utilities.Modulo(position.Z, Chunk.HorizontalSize);

        ref Chunk chunk = ref worldChunkManager.GetChunkRefOrNullRef(chunkPosition);

        if (Unsafe.IsNullRef(ref chunk))
            return BlockStates.VoidAir.Root.Default;

        return BlockStates.GetState(chunk.GetBlockState(minY, relativeX, position.Y, relativeZ).RegistryId);
    }

    public bool IsCurrentlyLoaded(BlockPosition position)
        => IsCurrentlyLoaded(position.ToChunkPosition());

    public bool IsCurrentlyLoaded(ChunkPosition position)
        => this.worldChunkManager.IsCurrentlyLoaded(position);

    public void Tick()
    {
        if (!this.tickingFlag.TrySet())
            ThrowHelpers.ThrowTickingWhileTicked();

        try
        {
            // EntityManager.Tick();
            LogicSyncTick();
            ++this.worldAge;
        }
        catch(Exception e)
        {
            Console.WriteLine($"------ Exception while ticking world! -----\n{e}");
        }
        finally
        {
            this.tickingFlag.Reset();
        }
    }

    protected virtual void LogicSyncTick()
    {
    }

    public ValueTask DisposeAsync()
    {
        GC.SuppressFinalize(this);
        return this.worldChunkManager.DisposeAsync();
    }

    [Flags]
    public enum WorldFlags
    {
        None,
        IsClient = 1 << 0,
    }
}

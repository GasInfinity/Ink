using System.Collections.Concurrent;
using System.Runtime.InteropServices;
using Ink.Blocks;
using Ink.Blocks.State;
using Ink.Chunks;
using Ink.Entities;
using Ink.Math;
using Ink.Registries;
using Ink.Util;
using Rena.Mathematics;

namespace Ink.World;

public abstract class BaseWorld : ITickable, IDisposable
{
    public const int HorizontalSize = 30000000;
    public const int VerticalSize = 20000000;

    // FIXME: Maybe we don't need this and we can just use 2 dictionaries
    private readonly ConcurrentDictionary<int, Entity> allEntitiesByNetworkId = new();

    // FIXME: Create a WorldChunkManager, clients and servers handle them differently
    private readonly Dictionary<ChunkPosition, Chunk> allChunks = new();

    protected ThreadSafeFlag tickingFlag = new();
    protected long worldAge;

    public readonly WorldFlags Flags;
    public readonly Uuid Uuid;
    public readonly Identifier Dimension;
    public readonly DimensionType CachedDimensionType;
    public readonly FrozenRegistry<Block> BlockRegistry;

    protected abstract EntityManager EntityManager { get; }

    public IEnumerable<Entity> Entities
        => EntityManager.Entities;

    public GameDifficulty Difficulty { get; set; }

    public bool IsClient
        => Flags.HasFlag(WorldFlags.IsClient);

    protected BaseWorld(WorldFlags flags, Uuid uuid, FrozenRegistry<Block> blockRegistry, FrozenRegistry<DimensionType> dimensionRegistry, Identifier dimension)
    {
        Flags = flags;
        Uuid = uuid;
        BlockRegistry = blockRegistry;
        Dimension = dimension;
        CachedDimensionType = dimensionRegistry.Get(dimension)
                            ?? throw new ArgumentException($"Dimension '{dimension}' not found inside dimensions registry");
    }

    protected BaseWorld(WorldFlags flags, FrozenRegistry<Block> blockRegistry, FrozenRegistry<DimensionType> dimensionRegistry, Identifier dimension) : this(flags,  (Uuid)Guid.NewGuid(), blockRegistry, dimensionRegistry, dimension)
    {
    }

    // public CChunkDataUpdateLightPacket GetChunkPacket(ChunkPosition position)
    // {
    //     (Region region, RelativeChunkPosition relativeRegionPosition) = GetParsed(position);
    //     return region.GetChunkPacket(relativeRegionPosition);
    // }

    public bool IsValid(BlockPosition position)
        => IsValidHorizontally(position.X, position.Z) & IsValidVertically(position.Y);

    public bool IsInBuildLimit(BlockPosition position)
        => IsValidHorizontally(position.X, position.Z) && IsInBuildLimit(position.Y);

    public bool IsValidHorizontally(int x, int z) // (x >= -HorizontalSize & x < HorizontalSize) & (z >= -HorizontalSize & z < HorizontalSize)
        => ((uint)(x + HorizontalSize) < (HorizontalSize * 2))
         & ((uint)(z + HorizontalSize) < (HorizontalSize * 2));

    public bool IsValidVertically(int y) // (y >= -VerticalSize & y < VerticalSize)
        => ((uint)(y + VerticalSize) < (VerticalSize * 2));

    public bool IsInBuildLimit(int y)
    {
        int minY = CachedDimensionType.MinY;
        int maxY = (CachedDimensionType.Height - minY);
        return y >= minY & y < maxY;
    }

    public IEnumerable<Entity> NearbyEntities(BlockPosition position, int radius = 16) // TODO: Optimize this
        => EntityManager.NearbyEntities(position);

    public virtual void AddEntity(Entity entity)
    {
        if(this.allEntitiesByNetworkId.TryAdd(entity.EntityId, entity))
        {
            EntityManager.AddEntity(entity);
        }
    }

    public virtual void RemoveEntity(Entity entity)
    {
        if(this.allEntitiesByNetworkId.TryRemove(entity.EntityId, out _))
        {
            EntityManager.RemoveEntity(entity);
        }
    }

    public abstract TEntity SpawnEntity<TEntity>(in Vec3<double> spawnPosition)
        where TEntity : Entity, IEntityFactory<TEntity>;

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

        foreach (Entity entity in EntityManager.NearbyEntities(position))
        {
            if (!entity.ObstructsBlockPlacements)
                continue;

            Aabb entityCollider = entity.Box;
            if (false)//(blockCollider.Intersects(entityCollider))
            {
                possibleCollidingEntity = entity;
                return false;
            }
        }

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

        ref Chunk chunk = ref GetChunk(chunkPosition);

        if (!chunk.IsCreated)
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

        ref Chunk chunk = ref GetChunk(chunkPosition);

        if (!chunk.IsCreated)
            return BlockStates.VoidAir.Root.Default;

        return BlockStates.GetState(chunk.GetBlockState(minY, relativeX, position.Y, relativeZ).RegistryId);
    }

    /*
    public virtual async ValueTask SetBiomeAsync(BlockPosition position, BlockState state)
    {
        (Region region, BlockPosition relativeRegionPosition) = await GetParsedAsync(position);
        await region.SetBlock(relativeRegionPosition, state);
    }

    public async ValueTask<BlockState> GetBiomeAsync(BlockPosition position)
    {
        (Region region, BlockPosition relativeRegionPosition) = await GetParsedAsync(position);
        return await region.GetBlock(relativeRegionPosition);
    }*/

    public bool IsCurrentlyLoaded(BlockPosition position)
        => IsCurrentlyLoaded(position.ToChunkPosition());

    public bool IsCurrentlyLoaded(ChunkPosition position)
        => this.allChunks.ContainsKey(position);

    public void Tick()
    {
        if (!this.tickingFlag.TrySet())
            ThrowHelpers.ThrowTickingWhileTicked();

        try
        {
            EntityManager.Tick();
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

    public ref Chunk GetChunk(ChunkPosition position)
    {
        ref Chunk chunk = ref CollectionsMarshal.GetValueRefOrAddDefault(this.allChunks, position, out bool exists);

        if(!exists)
        {
            chunk = new (CachedDimensionType.MinY, CachedDimensionType.Height);

            for(int x = 0; x < 16; ++x)
            {
                for(int z = 0; z < 16; ++z)
                {
                    chunk.SetBlockState(CachedDimensionType.MinY, x, 0, z, new(Random.Shared.Next(BlockStates.StateCount)));
                }
            }
        }

        return ref chunk;
    }

    public void Dispose()
    {
        GC.SuppressFinalize(this);

        foreach (ChunkPosition chunkPosition in this.allChunks.Keys)
        {
            ref Chunk chunk = ref CollectionsMarshal.GetValueRefOrNullRef(this.allChunks, chunkPosition);
            chunk.Dispose();
        }
    }

    [Flags]
    public enum WorldFlags
    {
        None,
        IsClient = 1 << 0,
    }
}

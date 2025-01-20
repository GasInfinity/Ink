using Ink.Blocks;
using Ink.Blocks.State;
using Ink.Chunks;
using Ink.Entities;
using Ink.Math;
using Ink.Net.Packets.Play;
using Ink.Server.Net;
using Ink.Registries;
using Ink.Server.Entities;
using Ink.Util;
using Ink.World;
using Rena.Mathematics;
using System.Collections.Concurrent;

namespace Ink.Server.World;

public sealed class ServerWorld : BaseWorld
{
    const int SyncBlockUpdatesClearInterval = 200;

    private readonly ConcurrentDictionary<int, ServerPlayerEntity> worldPlayers = new();
    private readonly ConcurrentBag<(int, WorldEvent, BlockPosition, int)> syncWorldEvents = [];
    private readonly Dictionary<SectionPosition, ConcurrentBag<SectionBlockUpdate>> syncBlockUpdates = new();
    private readonly EntityManager entityManager = new();

    public IEnumerable<ServerPlayerEntity> Players
        => this.worldPlayers.Select(kv => kv.Value); // TODO: Optimized ConcurrentDictionary? .Values allocates each time its called

    protected override EntityManager EntityManager
        => this.entityManager;

    public readonly ServerRegistryManager RegistryManager;
    public ServerWorld(Uuid uuid, ServerRegistryManager registryManager, FrozenRegistry<Block> blockRegistry, FrozenRegistry<DimensionType> dimensionRegistry, Identifier dimension) : base(WorldFlags.None, uuid, blockRegistry, dimensionRegistry, dimension)
    {
        RegistryManager = registryManager;
    }

    public ServerWorld(ServerRegistryManager registryManager, FrozenRegistry<Block> blockRegistry, FrozenRegistry<DimensionType> dimensionRegistry, Identifier dimension) : base(WorldFlags.None, blockRegistry, dimensionRegistry, dimension)
    {
        RegistryManager = registryManager;
    }

    public override TEntity SpawnEntity<TEntity>(in Vec3<double> spawnPosition)
    {
        TEntity entity = TEntity.Create(DefaultEntityTrackerFactory.Shared); // TODO: This shouldn't be like this!
        entity.SetWorld(this, spawnPosition, default);
        return entity;
    }

    public override void SyncWorldEvent(Entity? entity, WorldEvent id, BlockPosition position, int data)
        => this.syncWorldEvents.Add(((entity?.EntityId ?? 0) + 1, id, position, data));

    public override bool SetBlockState(BlockPosition position, in BlockState state, BlockStateChangeFlags flags, int maxUpdateDepth)
    {
        bool changed = base.SetBlockState(position, state, flags, maxUpdateDepth);

        if (changed)
            AddSyncBlockUpdate(position, state.Id);

        return changed;
    }

    protected override void LogicSyncTick()
    {
        HandleWorldEvents();

        foreach (KeyValuePair<SectionPosition, ConcurrentBag<SectionBlockUpdate>> syncSectionUpdates in this.syncBlockUpdates)
            HandleSyncBlockUpdates(syncSectionUpdates.Key, syncSectionUpdates.Value);

        if ((this.worldAge % SyncBlockUpdatesClearInterval) == 0) // Do not store unused ConcurrentBags!
            this.syncBlockUpdates.Clear();
    }

    public override void AddEntity(Entity entity)
    {
        base.AddEntity(entity);

        if (entity is ServerPlayerEntity player)
        {
            _ = this.worldPlayers.TryAdd(entity.EntityId, player);
        }
    }

    public override void RemoveEntity(Entity entity)
    {
        base.RemoveEntity(entity);

        _ = this.worldPlayers.Remove(entity.EntityId, out _);
    }

    private void AddSyncBlockUpdate(BlockPosition position, StateStorage state)
    {
        SectionPosition sectionPosition = position.ToSectionPosition();

        int relX = position.X & 0xF;
        int relY = position.Y & 0xF;
        int relZ = position.Z & 0xF;

        if(!this.syncBlockUpdates.TryGetValue(sectionPosition, out ConcurrentBag<SectionBlockUpdate>? updates))
        {
            updates = new ();
            this.syncBlockUpdates.Add(sectionPosition, updates);
        }

        updates.Add(new SectionBlockUpdate(relX, relY, relZ, state.RegistryId));
    }

    private void HandleSyncBlockUpdates(SectionPosition position, ConcurrentBag<SectionBlockUpdate> blockUpdates)
    {
        if (blockUpdates.IsEmpty)
            return;

        ChunkPosition chunkPosition = position.ToChunkPosition();

        if (blockUpdates.Count == 1)
        {
            if (!blockUpdates.TryTake(out SectionBlockUpdate blockUpdate) && blockUpdates.IsEmpty)
                throw new InvalidOperationException($"BUG! '{nameof(syncBlockUpdates)}' modified while synchronizing block updates with clients."); // Sanity check

            BlockPosition absoluteBlockPosition = position.ToAbsolute(blockUpdate.RelX, blockUpdate.RelY, blockUpdate.RelZ);
            int blockState = blockUpdate.BlockState;

            ClientboundBlockUpdate singleUpdatePacket = new(absoluteBlockPosition, blockState);
            foreach (ServerPlayerEntity player in Players)
            {
                if (!player.IsChunkViewed(chunkPosition))
                    continue;

                ServerNetworkConnection connection = player.NetworkContext.Connection;
                connection.Send(singleUpdatePacket);
            }

            return;
        }

        long[] rawSectionBlockUpdates = GC.AllocateUninitializedArray<long>(blockUpdates.Count);

        int i = 0;
        while (blockUpdates.TryTake(out SectionBlockUpdate blockUpdate))
            rawSectionBlockUpdates[i++] = blockUpdate.Raw;

        ClientboundSectionBlocksUpdate updatePacket = new(position, rawSectionBlockUpdates);
        foreach (ServerPlayerEntity player in Players)
        {
            if (!player.IsChunkViewed(chunkPosition))
                continue;

            ServerNetworkConnection connection = player.NetworkContext.Connection;
            connection.Send(updatePacket);
        }
    }

    private void HandleWorldEvents()
    {
        while(this.syncWorldEvents.TryTake(out (int, WorldEvent, BlockPosition, int) data))
        {
            BlockPosition position = data.Item3;
            // CWorldEventPacket packet = new(data.Item2, position, data.Item4, false); // TODO

            int initiatorEntityId = data.Item1 - 1;
            ChunkPosition eventChunkLocation = position.ToChunkPosition();

            if (initiatorEntityId == -1)
            {
                foreach (ServerPlayerEntity player in Players)
                {
                    if (!player.IsChunkViewed(eventChunkLocation))
                        continue;

                    ServerNetworkConnection connection = player.NetworkContext.Connection;
                    // connection.Send(packet);
                }
            }
            else
            {
                foreach (ServerPlayerEntity player in Players)
                {
                    if (player.EntityId == initiatorEntityId || !player.IsChunkViewed(eventChunkLocation))
                        continue;

                    ServerNetworkConnection connection = player.NetworkContext.Connection;
                    // connection.Send(packet);
                }
            }
        }
    }
}

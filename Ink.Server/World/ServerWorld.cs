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
using Ink.Worlds;
using Rena.Mathematics;
using System.Collections.Concurrent;
using Friflo.Engine.ECS;
using Ink.Server.Entities.Components;
using Ink.Net;
using Friflo.Engine.ECS.Systems;
using Ink.Server.Worlds.Systems;

namespace Ink.Server.Worlds;

public sealed class ServerWorld : World
{
    const int SyncBlockUpdatesClearInterval = 200;

    private readonly ConcurrentBag<(int, WorldEvent, BlockPosition, int)> syncWorldEvents = [];
    private readonly Dictionary<SectionPosition, ConcurrentBag<SectionBlockUpdate>> syncBlockUpdates = new();

    private readonly ArchetypeQuery<EntityRemotePlayerComponent> remotePlayersQuery;
    private readonly ArchetypeQuery<EntityRemotePlayerComponent, EntityChunkViewerComponent> chunkRemotePlayersQuery;

    public readonly EntityStore Entities;
    public readonly SystemRoot Systems;

    private long ticks;

    public readonly ServerRegistryManager RegistryManager;
    public readonly ServerChunkManager ChunkManager;

    public ServerWorld(Uuid uuid, ServerChunkManager chunkManager, ServerRegistryManager registryManager, FrozenRegistry<Block> blockRegistry, FrozenRegistry<DimensionType> dimensionRegistry, Identifier dimension) : base(chunkManager, WorldFlags.None, uuid, blockRegistry, dimensionRegistry, dimension)
    {
        Entities = new();
        Systems = new(Entities) {
            new SystemGroup("common") {
                new RemoveSyncedEntitySystem(),
                new RemoveRemotePlayerSystem(),
                new DeletionSystem(),
                new ChunkViewingSystem(),
                new ViewingSystem(),
                new EntitySyncingSystem(),
                new SyncSpawnPositionSystem(),
                new SyncChunkCacheCenterSystem(),
            },
            new ChunkSenderSystem(this),
            new TransformToLastSystem(),
        };

        this.remotePlayersQuery = Entities.Query<EntityRemotePlayerComponent>();
        this.chunkRemotePlayersQuery = Entities.Query<EntityRemotePlayerComponent, EntityChunkViewerComponent>();

        Systems.SetMonitorPerf(true);
        RegistryManager = registryManager;
        ChunkManager = chunkManager;
    }

    public RemotePlayerEntity SpawnRemotePlayerEntity(ServerNetworkConnection connection)
    {
        return new(RemotePlayerEntity.Create(Entities.Batch(), connection).CreateEntity());
    }

    // public override void SyncWorldEvent(InkEntity? entity, WorldEvent id, BlockPosition position, int data)
    //     => this.syncWorldEvents.Add(((entity?.EntityId ?? 0) + 1, id, position, data));

    public override bool SetBlockState(BlockPosition position, in BlockState state, BlockStateChangeFlags flags, int maxUpdateDepth)
    {
        bool changed = base.SetBlockState(position, state, flags, maxUpdateDepth);

        if (changed)
            AddSyncBlockUpdate(position, state.Id);

        return changed;
    }

    protected override void LogicSyncTick()
    {
        Systems.Update(default);
        Console.Clear();
        Console.WriteLine(Systems.GetPerfLog());
        Console.WriteLine($"{GC.GetTotalMemory(false) / 1024f / 1024f:N3}MiB/{GC.GetTotalAllocatedBytes(false) / 1024f / 1024f:N3}MiB (GC Pause since server start: {GC.GetTotalPauseDuration()})");
        HandleWorldEvents();

        foreach (KeyValuePair<SectionPosition, ConcurrentBag<SectionBlockUpdate>> syncSectionUpdates in this.syncBlockUpdates)
            HandleSyncBlockUpdates(syncSectionUpdates.Key, syncSectionUpdates.Value);

        if ((this.worldAge % SyncBlockUpdatesClearInterval) == 0) // Do not store unused ConcurrentBags!
            this.syncBlockUpdates.Clear();
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
            // foreach (ServerPlayerEntity player in Players)
            // {
            //     if (!player.IsChunkViewed(chunkPosition))
            //         continue;
            //
            //     ServerNetworkConnection connection = player.NetworkContext.Connection;
            //     connection.Send(singleUpdatePacket);
            // }

            return;
        }

        long[] rawSectionBlockUpdates = GC.AllocateUninitializedArray<long>(blockUpdates.Count);

        int i = 0;
        while (blockUpdates.TryTake(out SectionBlockUpdate blockUpdate))
            rawSectionBlockUpdates[i++] = blockUpdate.Raw;

        ClientboundSectionBlocksUpdate updatePacket = new(position, rawSectionBlockUpdates);
        // foreach (ServerPlayerEntity player in Players)
        // {
        //     if (!player.IsChunkViewed(chunkPosition))
        //         continue;
        //
        //     ServerNetworkConnection connection = player.NetworkContext.Connection;
        //     connection.Send(updatePacket);
        // }
    }

    private void HandleWorldEvents()
    {
        while(this.syncWorldEvents.TryTake(out (int, WorldEvent, BlockPosition, int) data))
        {
            BlockPosition position = data.Item3;
            // CWorldEventPacket packet = new(data.Item2, position, data.Item4, false); // TODO

            int initiatorEntityId = data.Item1 - 1;
            ChunkPosition eventChunkLocation = position.ToChunkPosition();

            // if (initiatorEntityId == -1)
            // {
            //     foreach (ServerPlayerEntity player in Players)
            //     {
            //         if (!player.IsChunkViewed(eventChunkLocation))
            //             continue;
            //
            //         ServerNetworkConnection connection = player.NetworkContext.Connection;
            //         // connection.Send(packet);
            //     }
            // }
            // else
            // {
            //     foreach (ServerPlayerEntity player in Players)
            //     {
            //         if (player.EntityId == initiatorEntityId || !player.IsChunkViewed(eventChunkLocation))
            //             continue;
            //
            //         ServerNetworkConnection connection = player.NetworkContext.Connection;
            //         // connection.Send(packet);
            //     }
            // }
        }
    }
}

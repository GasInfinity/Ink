using Ink.Entities;
using Ink.World;
using Ink.Net;
using Ink.Net.Packets.Play;
using Ink.Server.Net;
using Ink.Chat;
using System.Diagnostics;
using Ink.Blocks.State;
using Ink.Server.World;
using Rena.Native.Extensions;
using Ink.Items;
using Ink.Blocks;
using Ink.Math;
using Rena.Mathematics;
using Ink.Containers;
using Ink.Text;
using Ink.Registries;
using Ink.Auth;

namespace Ink.Server.Entities;

public sealed class ServerPlayerEntity : PlayerEntity
{
    public static readonly TextPart RemovedFromWorldPart = TextPart.String("Removed from world by bad programming");

    private int currentTeleportId = -1;

    public readonly ServerNetworkConnection.ServerConnectionContext NetworkContext;
    private readonly ServerChunkSender chunkSender = new();
    private readonly ServerPlayerChunkManager chunkManager = new();
    // TODO: This must be private!
    public readonly PlayerContainer Inventory;
    public readonly PlayerContainerViewHandler PlayerViewHandler;
    public InventoryViewHandler InventoryViewHandler;

    private bool initialized;
    // private ImmutableArray<CPlayerInfoUpdatePacket.Entry> infoEntry;
    private Identifier currentDimension;
    private int lastTeleportSent;
    private int lastTeleportReplied = -1;
    private float lastSynchedHealth;
    private int lastSyncedExperience;
    private byte lastSyncedHeldSlot;
    private int acknowledgeBlocksTop = -1;
    private ServerPlayerFlags tickFlags;

    private int NextTeleportId
        => Interlocked.Increment(ref currentTeleportId);

    public bool ShouldDisplayOnListings { get; private set; } = true;
    public ChatMode Chat { get; private set; } = ChatMode.Enabled;
    public bool WantsChatColors { get; private set; } = true;
    public int ViewDistance { get; private set; } = 4;
    public BlockPosition SpawnPosition { get; private set; }
    public float SpawnYaw { get; private set; }

    public int LastTeleportSent
        => this.lastTeleportSent;

    public new ServerWorld World
        => base.World!.CastUnsafe<ServerWorld>();

    public override ItemStack this[EquipmentSlot slot] { get => Inventory[slot]; set => Inventory[slot] = value; }

    public ServerPlayerEntity(ServerNetworkConnection.ServerConnectionContext connection, GameProfile profile) : base(profile, DefaultEntityTrackerFactory.Shared)
    {
        NetworkContext = connection;
        Inventory = new();

        CurrentGameMode = GameMode.Creative;
        this.position = new(0, 16, 0);

        // this.infoEntry = [new CPlayerInfoUpdatePacket.Entry()
        // {
        //     Uuid = uuid,
        //     Username = username,
        //     GameMode = CurrentGameMode,
        //     Listed = ShouldDisplayOnListings,
        //     DisplayName = this.customName.Value,
        // }];

        PlayerViewHandler = new(this, Inventory);
        InventoryViewHandler = PlayerViewHandler;
    }

    public void SendSystemMessage(TextPart message)
    {
        NetworkContext.Connection.Send(new ClientboundSystemChat(message, false));
    }

    public void SendActionBar(TextPart actionBar)
    {
        NetworkContext.Connection.Send(new ClientboundSystemChat(actionBar, true));
    }

    public void SetTabHeaderFooter(TextPart header, TextPart footer)
        => NetworkContext.Connection.Send(new ClientboundTabList(header, footer));

    internal void Initialize(BaseWorld world)
    {
        Debug.Assert(!this.initialized);
        this.currentDimension = world.Dimension;

        // TODO: Move this to the Play Handler context somehow, this method shouldn't exist here 
        NetworkContext.Connection.Send(new ClientboundLogin(
            EntityId: EntityId,
            IsHardcore: false,
            DimensionNames: NetworkContext.GameHandler.RegistryManager.DimensionType.Keys.ToArray(),
            MaxPlayers: 0,
            ViewDistance: 4,
            SimulationDistance: 4,
            ReducedDebugInfo: false,
            EnableRespawnScreen: true,
            DoLimitedCrafting: false,
            DimensionType: NetworkContext.GameHandler.RegistryManager.DimensionType.GetId(this.currentDimension),
            DimensionName: this.currentDimension,
            HashedSeed: 0,
            GameMode: CurrentGameMode,
            PreviousGameMode: GameMode.Undefined,
            IsDebug: false,
            IsFlat: false,
            HasDeathLocation: false,
            DeathDimensionName: null,
            DeathLocation: null,
            PortalCooldown: 0,
            SeaLevel: 64,
            EnforcesSecureChat: false
        ));

        NetworkContext.Connection.Send(new ClientboundGameEvent(13, 0)); // TODO: Enum (StartWaitingForLevelChunks)

        // Connection.GameHandler.BroadcastPlay(new CPlayerInfoUpdatePacket(CPlayerInfoUpdatePacket.Actions.AddPlayer | CPlayerInfoUpdatePacket.Actions.UpdateListed | CPlayerInfoUpdatePacket.Actions.UpdateGameMode, this.infoEntry));

        // foreach (ServerPlayerEntity p in Connection.GameHandler.Playing)
        // {
        //     if (p == this)
        //         continue;
        //
        //     // Connection.Send(new CPlayerInfoUpdatePacket(CPlayerInfoUpdatePacket.Actions.AddPlayer | CPlayerInfoUpdatePacket.Actions.UpdateListed | CPlayerInfoUpdatePacket.Actions.UpdateGameMode, p.InfoEntry)); // TODO: Send all of them in one packet!
        // }

        SetWorld(world, position, default);
        this.initialized = true;
    }

    public override void SetWorld(BaseWorld newWorld, in Vec3<double> spawnPosition, Vec2<float> spawnRotation)
    {
        this.world = newWorld;

        BlockPosition spawnBlockPosition = (BlockPosition)spawnPosition;

        SetSpawnPosition(spawnBlockPosition, spawnRotation.X);
        Teleport(spawnPosition, spawnRotation);

        this.chunkSender.CurrentWorld = newWorld;
    }

    public override void Teleport(in Vec3<double> position, Vec2<float> rotation)
    {
        this.position = position;
        this.rotation = rotation;

        NetworkContext.Connection.Send(new ClientboundPlayerPosition(
            TeleportId: this.lastTeleportSent,
            X: position.X,
            Y: position.Y,
            Z: position.Z,
            VelocityX: Velocity.X,
            VelocityY: Velocity.Y,
            VelocityZ: Velocity.Z,
            Yaw: rotation.X,
            Pitch: rotation.Y,
            Flags: 0
        ));
    }

    public void SetSpawnPosition(BlockPosition newSpawn, float yaw)
    {
        SpawnPosition = newSpawn;
        NetworkContext.Connection.Send(new ClientboundSetDefaultSpawnPosition(newSpawn, yaw));
    }

    public bool TryBreakBlock(BlockPosition position)
    {
        BlockStateChild state = World.GetBlockState(position);
        Block? block = state.GetBlock(World.BlockRegistry);
        block?.OnBreak(World, position, state, this);
        return World.BreakBlock(position, true, this);
    }

    public void AcknowledgeChunkBatch(float chunksPerTick)
        => this.chunkSender.AcknowledgeChunkBatch(chunksPerTick);

    public void AcknowledgeHeldSlot(int heldSlot)
    {
        if(this.lastSyncedHeldSlot != Inventory.HeldSlot)
            return;

        this.lastSyncedHeldSlot = (byte)(Inventory.HeldSlot = heldSlot);
    }

    public void SetPlayerFlag(ServerPlayerFlags flag)
        => this.tickFlags |= flag;

    public override void Swing(Hand hand = Hand.Main)
        => this.tickFlags |= hand == Hand.Main ? ServerPlayerFlags.ShouldSwingMain : ServerPlayerFlags.ShouldSwingOff;

    protected override void TickLogic()
    {
        ProcessChunks();
        SyncChangedAttributes();
        ProcessFlags();

        GCMemoryInfo gcInfo = GC.GetGCMemoryInfo(GCKind.Any);

        if((this.ticks % 60) == 0)
            SetTabHeaderFooter(TextPart.String($"Ink Server :D [Running on .NET {Environment.Version}]"), TextPart.String($"{BlockStates.StateCount} BlockStates loaded in memory\n{GC.GetTotalMemory(false) / 1024f / 1024f:N3}MiB/{GC.GetTotalAllocatedBytes(false) / 1024f / 1024f:N3}MiB (GC Pause since server start: {GC.GetTotalPauseDuration()})\n Viewing current chunk: {this.chunkManager.IsViewing(((BlockPosition)this.position).ToChunkPosition())}"));
    }

    protected override void TickPhysics() // TODO
    {
        //base.TickPhysics();
    }

    private void ProcessChunks()
    {
        BlockPosition blockPosition = (BlockPosition)this.position;
        BlockPosition lastBlockPosition = (BlockPosition)this.lastPosition;

        SectionPosition sectionPosition = blockPosition.ToSectionPosition();
        if(sectionPosition != lastBlockPosition.ToSectionPosition())
        {
            NetworkContext.Connection.Send(new ClientboundSetChunkCacheCenter(sectionPosition.X, sectionPosition.Z));
        }

        this.chunkManager.UpdateChunks(this);

        foreach(ChunkPosition position in this.chunkManager.UnloadedChunks)
        {
            this.chunkSender.RemoveChunk(position, NetworkContext.Connection);
        }

        foreach(ChunkPosition position in this.chunkManager.NewChunks)
        {
            this.chunkSender.AddChunk(position);
        }

        this.chunkSender.SendChunks(NetworkContext.Connection);
    }

    public bool IsChunkViewed(ChunkPosition position)
        => this.chunkManager.IsViewing(position);

    private void SyncChangedAttributes()
    {
        byte currentHeldSlot = (byte)Inventory.HeldSlot;
        if(Inventory.HeldSlot != this.lastSyncedHeldSlot)
        {
            NetworkContext.Connection.Send(new ClientboundSetHeldSlot(currentHeldSlot));
            this.lastSyncedHeldSlot = currentHeldSlot;
        }

        if(lastSynchedHealth != this.health.Value)
        {
            NetworkContext.Connection.Send(new ClientboundSetHealth(this.health.Value, 20, 5));
            this.lastSynchedHealth = this.health.Value;
        }
    }

    private void ProcessFlags()
    {
        if (this.tickFlags.HasFlag(ServerPlayerFlags.ShouldSwingMain))
        {
            ClientboundAnimate swingMainPacket = new(EntityId, 0); // TODO: Animations enum

            Tracker.Send(swingMainPacket);

            if (!this.tickFlags.HasFlag(ServerPlayerFlags.ClientSwingedMain))
                NetworkContext.Connection.Send(swingMainPacket);
        }

        if (this.tickFlags.HasFlag(ServerPlayerFlags.ShouldSwingOff))
        {
            ClientboundAnimate swingOffPacket = new(EntityId, 3);

            Tracker.Send(swingOffPacket);
            if (!this.tickFlags.HasFlag(ServerPlayerFlags.ClientSwingedOff))
                NetworkContext.Connection.Send(swingOffPacket);
        }

        if(this.tickFlags.HasFlag(ServerPlayerFlags.SyncronizePosition))
        {
            Teleport(position, rotation);
        }

        this.tickFlags = default;
    }

    public override void Remove()
    {
        base.Remove();

        if (!NetworkContext.Connection.IsConnected)
            return;

        NetworkContext.Connection.Send(new ClientboundStartConfiguration());
        NetworkContext.SwitchState(NetworkState.Configuration);
    }


    public override string ToString()
    {
        return $"(ServerPlayer = {Profile})";
    }

    [Flags]
    public enum ServerPlayerFlags
    {
        None,
        ClientSwingedMain = 1 << 0,
        ClientSwingedOff = 1 << 1,
        ShouldSwingMain = 1 << 2,
        ShouldSwingOff = 1 << 3,
        SyncronizePosition = 1 << 4
    }
}

using Ink.Chunks;
using Ink.Entities;
using Ink.Net;
using Ink.Net.Packets.Play;
using Ink.Server.Net;
using Ink.Server.World;
using Ink.Util;
using Ink.Util.Extensions;
using Rena.Mathematics;
using Rena.Native.Extensions;
using System.Diagnostics;

namespace Ink.Server.Entities;

public class EntityTracker : IEntityTracker // TODO Split / Refactor this some day
{
    const int GlobalPositionSyncInterval = 64;

    protected readonly Entity entity;
    private readonly bool isLiving;
    private readonly bool isServerPlayer;
    private readonly bool consistentUpdates;
    private readonly uint tickSyncInterval;
    private HashSet<ServerPlayerEntity> viewers = [];
    private HashSet<ServerPlayerEntity> newViewers = [];
    private Vec3<double> lastSynchedPosition;
    private Vec3<double> lastSynchedVelocity;
    private Vec2<float> lastSynchedRotation;
    private float lastSyncedHeadYaw;
    private uint ticks;

    public EntityTracker(Entity entity)
    {
        this.entity = entity;
        this.isLiving = entity is LivingEntity;
        this.isServerPlayer = entity is ServerPlayerEntity;
        this.consistentUpdates = entity.Definition.ConsistentSyncUpdates;
        this.tickSyncInterval = entity.Definition.TickSyncInterval;
        this.lastSynchedPosition = entity.Position;
        this.lastSynchedRotation = entity.Rotation;
        this.lastSynchedVelocity = entity.Velocity;
        this.lastSyncedHeadYaw = (entity as LivingEntity)?.CurrentHeadYaw ?? 0;
    }

    public void Tick() // FIXME: Simplify this when the JIT gets Loop Unswitching
    {
        ServerWorld world = (this.entity.World as ServerWorld)!;
        Debug.Assert(world != null);

        Vec3<double> currentEntityPosition = this.entity.Position;
        Vec2<float> currentEntityRotation = this.entity.Rotation;
        Vec3<double> currentEntityVelocity = this.entity.Velocity;

        if(this.viewers.Count == 0)
        {
            this.lastSynchedPosition = currentEntityPosition;
            this.lastSynchedRotation = currentEntityRotation;
            this.lastSynchedVelocity = currentEntityVelocity;
            this.lastSyncedHeadYaw = (entity as LivingEntity)?.CurrentHeadYaw ?? 0;
        }
        else
        {
            foreach (ServerPlayerEntity viewer in this.viewers)
            {
                if (!InRange(viewer, currentEntityPosition))
                {
                    SendDestroy(viewer.NetworkContext.Connection);
                }
                else
                {
                    this.newViewers.Add(viewer);
                }
            }
        }

        bool hasDirtyMeta = this.entity.HasDirtyMetadata;
        ClientboundSetEntityData metaPacket = default;
        if (hasDirtyMeta)
        {
            // metaPacket = CSetEntityMetadataPacket.FromDirtyMeta(this.entity.EntityId, this.entity);

            if (isServerPlayer)
            {
                ServerPlayerEntity thisEntity = this.entity.CastUnsafe<ServerPlayerEntity>();
                ServerNetworkConnection connection = thisEntity.NetworkContext.Connection;
                // connection.Send(metaPacket);
            }
        }

        // ClientboundSetEntityData entityMetaPacket = CSetEntityMetadataPacket.FromMeta(this.entity.EntityId, this.entity);

        if (isLiving && entity.CastUnsafe<LivingEntity>().HasSynchedEquipment)
        {
            // ClientboundSetEntityData equipmentPacket = CSetEquipmentPacket.FromEquipment(this.entity.EntityId, entity.CastUnsafe<LivingEntity>().LastSynchedEquipment);

            foreach (ServerPlayerEntity player in world.Players)
            {
                if (this.entity != player && InRange(player, currentEntityPosition) && this.newViewers.Add(player))
                    SendFullSpawnWithEquipment(player.NetworkContext.Connection, default, default);
            }
        }
        else
        {
            foreach (ServerPlayerEntity player in world.Players)
            {
                if (this.entity != player && InRange(player, currentEntityPosition) && this.newViewers.Add(player))
                    SendFullSpawnWithoutEquipment(player.NetworkContext.Connection, default);
            }
        }

        (this.viewers, this.newViewers) = (this.newViewers, this.viewers);
        this.newViewers.Clear();

        if(this.viewers.Count > 0)
            UpdateViewers(currentEntityPosition, currentEntityRotation, currentEntityVelocity, hasDirtyMeta, metaPacket);

        ++this.ticks;
    }

    public void Remove()
    {
        foreach (var player in viewers)
            SendDestroy(player.NetworkContext.Connection);

        this.viewers.Clear();
    }

    protected virtual void SendSpecializedSpawn(ServerNetworkConnection connection)
    {
        Vec3<short> velocity = Int16Velocity.ToVelocity(this.lastSynchedVelocity);
        connection.Send(new ClientboundAddEntity(this.entity.EntityId, this.entity.Uuid, this.entity.Definition.Type, this.lastSynchedPosition.X, this.lastSynchedPosition.Y, this.lastSynchedPosition.Z, (Angle)this.lastSynchedRotation.X, (Angle)this.lastSynchedRotation.Y, (Angle)this.lastSyncedHeadYaw, default, velocity.X, velocity.Y, velocity.Z)); // TODO: Entity Velocity and Data
    }

    protected void SendDestroy(ServerNetworkConnection connection)
        => connection.Send(new ClientboundRemoveEntities([this.entity.EntityId]));

    private bool InRange(ServerPlayerEntity player, in Vec3<double> currentEntityPosition)
        => player.NetworkContext.Connection.IsConnected && player.World == this.entity.World && player.Position.DistanceSqr(currentEntityPosition) < player.ViewDistance * player.ViewDistance * Chunk.HorizontalSurface;

    private void SendFullSpawnWithoutEquipment(ServerNetworkConnection connection, ClientboundSetEntityData cachedMeta)
    {
        connection.Send(new ClientboundBundleDelimiter());
        SendSpecializedSpawn(connection);
        connection.Send(cachedMeta);
        connection.Send(new ClientboundBundleDelimiter());
    }

    private void SendFullSpawnWithEquipment(ServerNetworkConnection connection, ClientboundSetEntityData cachedMeta, ClientboundSetEquipment cachedEquipment)
    {
        connection.Send(new ClientboundBundleDelimiter());
        SendSpecializedSpawn(connection);
        connection.Send(cachedMeta);
        connection.Send(cachedEquipment);
        connection.Send(new ClientboundBundleDelimiter());
    }

    private void UpdateViewers(in Vec3<double> position, in Vec2<float> rotation, in Vec3<double> velocity, bool hasDirtyMeta, in ClientboundSetEntityData dirtyMetaPacket) // When will the JIT have loop unswitching?
    {
        bool shouldUpdate = this.consistentUpdates
                         || hasDirtyMeta
                         || ((this.ticks % this.tickSyncInterval) == 0);

        if (!shouldUpdate)
            return;

        int eId = this.entity.EntityId;

        double deltaX = position.X - this.lastSynchedPosition.X;
        double deltaY = position.Y - this.lastSynchedPosition.Y;
        double deltaZ = position.Z - this.lastSynchedPosition.Z;

        double absDeltaX = double.Abs(deltaX);
        double absDeltaY = double.Abs(deltaY);
        double absDeltaZ = double.Abs(deltaZ);

        bool updatePosition = (absDeltaX + absDeltaY + absDeltaZ > 0) || ((this.ticks % GlobalPositionSyncInterval) == 0);
        bool updateRotation = this.lastSynchedRotation != rotation;
        bool updatePositionRotation = (updatePosition && updateRotation) || this.consistentUpdates;
        bool sendEntityTeleport = (absDeltaX > Int16Delta.MaxDelta || absDeltaY > Int16Delta.MaxDelta || absDeltaZ > Int16Delta.MaxDelta);
        bool updateVelocity = this.lastSynchedVelocity != velocity;
        bool onGround = this.entity.OnGround;

        short shortDeltaX = Int16Delta.ToDelta(deltaX);
        short shortDeltaY = Int16Delta.ToDelta(deltaY);
        short shortDeltaZ = Int16Delta.ToDelta(deltaZ);

        byte currentYawAngle = Angle.ToAngle(rotation.X);
        byte currentPitchAngle = Angle.ToAngle(rotation.Y);

        Vec3<short> shortVelocity = Int16Velocity.ToVelocity(velocity);

        Send(new ClientboundBundleDelimiter());

        if (hasDirtyMeta)
        {
            Send(dirtyMetaPacket);
        }

        if (sendEntityTeleport)
        {
            Send(new ClientboundEntityPositionSync(eId, position.X, position.Y, position.Z, shortVelocity.X, shortVelocity.Y, shortVelocity.Z, rotation.X, rotation.Y, onGround));
        }
        else if (updatePositionRotation)
        {
            Send(new ClientboundMoveEntityPosRot(eId, shortDeltaX, shortDeltaY, shortDeltaZ, (Angle)currentYawAngle, (Angle)currentPitchAngle, onGround));
        }
        else if (updateRotation)
        {
            Send(new ClientboundMoveEntityRot(eId, (Angle)currentYawAngle, (Angle)currentPitchAngle, onGround));
        }
        else if (updatePosition)
        {
            Send(new ClientboundMoveEntityPos(eId, shortDeltaX, shortDeltaY, shortDeltaZ, onGround));
        }

        if(updateVelocity)
        {
            Send(new ClientboundSetEntityMotion(eId, shortVelocity.X, shortVelocity.Y, shortVelocity.Z));
        }

        if (isLiving)
        {
            float currentHeadYaw = entity.CastUnsafe<LivingEntity>().CurrentHeadYaw;

            if (!this.lastSyncedHeadYaw.AlmostEqual(currentHeadYaw))
            {
                Send(new ClientboundRotateHead(eId, (Angle)currentHeadYaw));
                this.lastSyncedHeadYaw = currentHeadYaw;
            }
        }

        Send(new ClientboundBundleDelimiter());

        this.lastSynchedPosition = position;
        this.lastSynchedRotation = rotation;
        this.lastSynchedVelocity = velocity;
    }

    public void Send<TPacket>(in TPacket packet)
        where TPacket : struct, IPacket<TPacket>
    {
        static void SendPacket(ServerNetworkConnection connection, TPacket packet) // Generic virtuals are never inlined. tf, it isn't even a virtual call...
            => connection.Send(packet);

        foreach (ServerPlayerEntity player in this.viewers)
        {
            ServerNetworkConnection connection = player.NetworkContext.Connection;
            SendPacket(connection, packet);
        }
    }
}

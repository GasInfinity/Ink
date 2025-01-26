using Friflo.Engine.ECS;
using Friflo.Engine.ECS.Systems;
using Ink.Entities.Components;
using Ink.Net.Packets.Play;
using Ink.Server.Entities.Components;
using Ink.Server.Net;
using Ink.Util;
using Rena.Mathematics;

namespace Ink.Server.Worlds.Systems;

// TODO: This could be pararellized. Will benefit GREATLY from it...
public sealed class EntitySyncingSystem : QuerySystem<EntityIdComponent, EntityTransformComponent, EntityViewedComponent, EntitySyncedComponent>
{
    protected override void OnUpdate()
    {
        Query.Each(new SyncEach(Query.Store)); 
    }

    private struct SyncEach(EntityStore store) : IEach<EntityIdComponent, EntityTransformComponent, EntityViewedComponent, EntitySyncedComponent>
    {
        public void Execute(ref EntityIdComponent id, ref EntityTransformComponent transform, ref EntityViewedComponent viewed, ref EntitySyncedComponent sync)
        {
            int networkId = id.NetworkId;

            foreach(int lastViewerId in viewed.LastViewers)
            {
                if(!viewed.Viewers.Contains(lastViewerId))
                {
                    Entity lastViewer = store.GetEntityById(lastViewerId);
                    ref EntityRemotePlayerComponent remote = ref lastViewer.GetComponent<EntityRemotePlayerComponent>(); 
                    ServerNetworkConnection connection = remote.Connection;

                    connection.Send(new ClientboundRemoveEntities([networkId]));
                }
            }

            if(viewed.Viewers.Count <= 0)
                return;

            Vec3<double> position = transform.Position;
            Vec2<float> rotation = transform.Rotation;
            Vec3<double> velocity = transform.Velocity;
            float headYaw = transform.HeadYaw;

            double deltaX = position.X - sync.LastSyncedPosition.X;
            double deltaY = position.Y - sync.LastSyncedPosition.Y;
            double deltaZ = position.Z - sync.LastSyncedPosition.Z;

            double absDeltaX = double.Abs(deltaX);
            double absDeltaY = double.Abs(deltaY);
            double absDeltaZ = double.Abs(deltaZ);

            bool updatePosition = (absDeltaX + absDeltaY + absDeltaZ > 0);// || ((this.ticks % GlobalPositionSyncInterval) == 0);
            bool updateRotation = sync.LastSyncedRotation != rotation;
            bool updatePositionRotation = (updatePosition && updateRotation);// || this.consistentUpdates;
            bool updateHeadYaw = sync.LastSyncedHeadYaw != headYaw;
            bool sendEntityTeleport = (absDeltaX > Int16Delta.MaxDelta || absDeltaY > Int16Delta.MaxDelta || absDeltaZ > Int16Delta.MaxDelta);
            bool updateVelocity = sync.LastSyncedVelocity != velocity;
            bool onGround = true;//this.entity.OnGround;

            short shortDeltaX = Int16Delta.ToDelta(deltaX);
            short shortDeltaY = Int16Delta.ToDelta(deltaY);
            short shortDeltaZ = Int16Delta.ToDelta(deltaZ);

            Angle currentYawAngle = new(rotation.X);
            Angle currentPitchAngle = new(rotation.Y);
            Angle currentHeadYawAngle = new(headYaw);

            Vec3<short> shortVelocity = Int16Velocity.ToVelocity(velocity);

            ClientboundAddEntity cachedAddEntity = new ClientboundAddEntity(
                EntityId: networkId,
                EntityUuid: id.Id,
                Type: 147, // Player Entity ID.  TODO: This should not be hardcoded, maybe make another component? 
                X: sync.LastSyncedPosition.X,
                Y: sync.LastSyncedPosition.Y,
                Z: sync.LastSyncedPosition.Z,
                Yaw: new(sync.LastSyncedRotation.X),
                Pitch: new(sync.LastSyncedRotation.Y),
                HeadYaw: new(sync.LastSyncedHeadYaw),
                Data: 0,
                VelocityX: 0,
                VelocityY: 0,
                VelocityZ: 0
            );

            sync = new(position, velocity, rotation, headYaw);

            foreach(int viewerId in viewed.Viewers) 
            {
                Entity viewer = store.GetEntityById(viewerId);
                ref EntityRemotePlayerComponent remote = ref viewer.GetComponent<EntityRemotePlayerComponent>(); 
                ServerNetworkConnection connection = remote.Connection;

                connection.Send(new ClientboundBundleDelimiter());

                if(!viewed.LastViewers.Contains(viewerId))
                {
                    connection.Send(cachedAddEntity); 
                }

                if (sendEntityTeleport)
                {
                    connection.Send(new ClientboundEntityPositionSync(networkId, position.X, position.Y, position.Z, shortVelocity.X, shortVelocity.Y, shortVelocity.Z, rotation.X, rotation.Y, onGround));
                }
                else if (updatePositionRotation)
                {
                    connection.Send(new ClientboundMoveEntityPosRot(networkId, shortDeltaX, shortDeltaY, shortDeltaZ, currentYawAngle, currentPitchAngle, onGround));
                }
                else if (updateRotation)
                {
                    connection.Send(new ClientboundMoveEntityRot(networkId, currentYawAngle, currentPitchAngle, onGround));
                }
                else if (updatePosition)
                {
                    connection.Send(new ClientboundMoveEntityPos(networkId, shortDeltaX, shortDeltaY, shortDeltaZ, onGround));
                }
            
                if(updateHeadYaw)
                {
                    connection.Send(new ClientboundRotateHead(networkId, currentHeadYawAngle));
                }

                connection.Send(new ClientboundBundleDelimiter());
            }
            
        }
    }
}

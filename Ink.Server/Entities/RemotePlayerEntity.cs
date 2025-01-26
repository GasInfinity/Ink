using Friflo.Engine.ECS;
using Ink.Entities;
using Ink.Server.Entities.Components;
using Ink.Server.Net;

namespace Ink.Server.Entities;

public readonly struct RemotePlayerEntity(Entity Entity)
{
    public readonly Entity Entity = Entity;

    public ServerNetworkConnection Connection
        => Entity.GetComponent<EntityRemotePlayerComponent>().Connection;

    public ref EntityChunkSenderComponent ChunkSender
        => ref Entity.GetComponent<EntityChunkSenderComponent>();

    public ref int ViewDistance
        => ref Entity.GetComponent<EntityChunkViewerComponent>().Distance;

    public PlayerEntity Player
        => new(Entity);

    public static CreateEntityBatch Create(CreateEntityBatch batch, ServerNetworkConnection connection)
        => PlayerEntity.Create(batch, connection.Context.Profile)
        .Add(new EntityRemotePlayerComponent(connection))
        .Add(new EntityChunkViewerComponent(4))
        .Add(new EntityChunkSenderComponent(8f))
        .Add(new EntityLastSyncedSpawnPositionComponent())
        .Add(new EntityViewedComponent(new(), new()))
        .Add(new EntitySyncedComponent());
}

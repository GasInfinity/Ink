using Friflo.Engine.ECS;
using Friflo.Engine.ECS.Systems;
using Ink.Entities.Components;
using Ink.Net.Packets.Play;
using Ink.Server.Entities.Components;

namespace Ink.Server.Worlds.Systems;

public sealed class SyncSpawnPositionSystem : QuerySystem<EntityRemotePlayerComponent, EntitySpawnPositionComponent, EntityLastSyncedSpawnPositionComponent>
{
    protected override void OnUpdate()
        => Query.Each(new SendPositionEach());

    private struct SendPositionEach : IEach<EntityRemotePlayerComponent, EntitySpawnPositionComponent, EntityLastSyncedSpawnPositionComponent>
    {
        public void Execute(ref EntityRemotePlayerComponent remote, ref EntitySpawnPositionComponent spawn, ref EntityLastSyncedSpawnPositionComponent lastSpawn)
        {
            if(spawn.Location == lastSpawn.Location && spawn.Yaw == lastSpawn.Yaw)
                return;

            remote.Connection.Send(new ClientboundSetDefaultSpawnPosition(spawn.Location, spawn.Yaw));
        }
    }
}

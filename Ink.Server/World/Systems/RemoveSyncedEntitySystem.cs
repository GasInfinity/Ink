using Friflo.Engine.ECS;
using Friflo.Engine.ECS.Systems;
using Ink.Entities.Components;
using Ink.Net.Packets.Play;
using Ink.Server.Entities.Components;
using Ink.Server.Net;

namespace Ink.Server.Worlds.Systems;

public sealed class RemoveSyncedEntitySystem : QuerySystem<EntityIdComponent, EntityViewedComponent, EntitySyncedComponent>
{
    public RemoveSyncedEntitySystem() => Filter.WithDisabled().AllTags(Tags.Get<Disabled>());

    protected override void OnUpdate()
    {
        Query.Each(new RemoveEach(Query.Store)); 
    }

    private struct RemoveEach(EntityStore store) : IEach<EntityIdComponent, EntityViewedComponent, EntitySyncedComponent>
    {
        public void Execute(ref EntityIdComponent id, ref EntityViewedComponent viewed, ref EntitySyncedComponent _)
        {
            int networkId = id.NetworkId;
            int[] networkIdArray = [networkId];

            foreach(int viewerId in viewed.Viewers)
            {
                Entity viewer = store.GetEntityById(viewerId);
                ref EntityRemotePlayerComponent remote = ref viewer.GetComponent<EntityRemotePlayerComponent>(); 
                ServerNetworkConnection connection = remote.Connection;

                connection.Send(new ClientboundRemoveEntities(networkIdArray));
            }
        }
    }
}

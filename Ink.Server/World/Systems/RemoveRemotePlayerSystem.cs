using Friflo.Engine.ECS;
using Friflo.Engine.ECS.Systems;
using Ink.Net;
using Ink.Net.Packets.Play;
using Ink.Server.Entities.Components;
using Ink.Server.Net;

namespace Ink.Server.Worlds.Systems;

public sealed class RemoveRemotePlayerSystem : QuerySystem<EntityRemotePlayerComponent>
{
    public RemoveRemotePlayerSystem() => Filter.WithDisabled().AllTags(Tags.Get<Disabled>());

    protected override void OnUpdate()
    {
        Query.Each(new RemoveRemotePlayerEach());
    }

    private readonly struct RemoveRemotePlayerEach : IEach<EntityRemotePlayerComponent>
    {
        public void Execute(ref EntityRemotePlayerComponent remotePlayer)
        {
            ServerNetworkConnection connection = remotePlayer.Connection;

            if(!connection.IsConnected)
                return;

            connection.Send(new ClientboundStartConfiguration());
            connection.Context.SwitchState(NetworkState.Configuration);
        }
    }
}

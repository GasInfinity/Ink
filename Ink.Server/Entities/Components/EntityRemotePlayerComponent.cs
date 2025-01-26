using Friflo.Engine.ECS;
using Ink.Server.Net;

namespace Ink.Server.Entities.Components;

public readonly record struct EntityRemotePlayerComponent(ServerNetworkConnection Connection) : IIndexedComponent<ServerNetworkConnection>
{
    public readonly ServerNetworkConnection Connection = Connection;

    public ServerNetworkConnection GetIndexedValue() => Connection;
}

using Ink.Server.Net;
using Ink.Server.Worlds;

namespace Ink.Server.Event;

public record struct LoginEvent(ServerNetworkConnection Connection)
{
    public readonly ServerNetworkConnection Connection = Connection;
    public ServerWorld? AssignedWorld = null;
}

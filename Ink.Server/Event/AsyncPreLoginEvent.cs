using Ink.Server.Net;

namespace Ink.Server.Event;

public readonly record struct PreLoginEvent(ServerNetworkConnection Connection, string Username)
{
    public readonly ServerNetworkConnection Connection = Connection;
    public readonly string Username = Username;
}

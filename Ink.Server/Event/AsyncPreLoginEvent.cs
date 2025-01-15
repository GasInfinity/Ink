using Ink.Server.Net;

namespace Ink.Server.Event;

public readonly record struct PreLoginEvent
{
    public ServerNetworkConnection Connection { get; init; }
    public string Username { get; init; }

    public PreLoginEvent(ServerNetworkConnection connection, string username)
        => (Connection, Username) = (connection, username);
}

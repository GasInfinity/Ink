namespace Ink.Server.Event;

public sealed class ServerEventManager
{
    public IAsyncPreLoginListener? PreLoginListener { get; set; }
}

using Ink.Server.Event;
using System.Net;

namespace Ink.Server;

public record InkServerOptions
{
    public required ILoginListener LoginListener { get; init; }
    public required Network NetworkOptions { get; init; }
    
    public record Network
    {
        public required EndPoint ListeningEndPoint { get; init; }
    }
}

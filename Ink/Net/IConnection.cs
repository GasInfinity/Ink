using System.Net;
using Ink.Text;

namespace Ink.Net;

public interface IConnection : ITickable
{
    string Id { get; }
    EndPoint? RemoteEndPoint { get; }
    bool IsConnected { get; }

    void Send<TPacket>(in TPacket packet)
        where TPacket : struct, IPacket<TPacket>;

    void Disconnect();
    void Disconnect(TextPart reason);

    void Abort();
    void Abort(TextPart reason);
}

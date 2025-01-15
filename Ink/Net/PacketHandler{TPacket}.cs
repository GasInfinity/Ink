namespace Ink.Net;

public abstract class PacketHandler<TContext, TPacket> : PacketHandler<TContext>
    where TPacket : struct, IPacket<TPacket>
{
    public sealed override bool TryHandle(ReadOnlySpan<byte> payload, IConnection connection, TContext ctx)
    {
        if(!TPacket.TryRead(payload, out TPacket packet))
            return false;

        Handle(in packet, connection, ctx);
        return true; 
    }

    public abstract void Handle(in TPacket packet, IConnection connection, TContext ctx);
}

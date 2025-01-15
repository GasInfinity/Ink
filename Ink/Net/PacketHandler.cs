namespace Ink.Net;

public abstract class PacketHandler<TContext>
{
    public abstract bool TryHandle(ReadOnlySpan<byte> payload, IConnection connection, TContext ctx);
}

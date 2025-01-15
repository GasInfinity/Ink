using System.Collections.Frozen;
using Ink.Registries;
using Ink.Text;

namespace Ink.Net;

public abstract class PacketStateHandler<TContext>(NetworkStateInfo stateInfo, NetworkDirection direction)
{
    public readonly NetworkStateInfo StateInfo = stateInfo;
    public readonly NetworkDirection Direction = direction;
    private Dictionary<int, PacketHandler<TContext>>? handlers = new();
    private FrozenDictionary<int, PacketHandler<TContext>> frozenHandlers = FrozenDictionary<int, PacketHandler<TContext>>.Empty;

    public virtual void Setup(IConnection connection, TContext ctx)
    {
    }

    public virtual void Tick(IConnection connection, TContext ctx)
    {
    }

    public virtual void CompressionEnabled(IConnection connection, TContext ctx, int newThreshold)
    {
    }

    public virtual void Disconnected(IConnection connection, TContext ctx, TextPart reason)
    {
    }

    public virtual void Terminated(IConnection connection, TContext ctx, TextPart reason)
    {
    }

    public PacketHandlingStatus TryHandle(int id, ReadOnlySpan<byte> payload, IConnection connection, TContext ctx)
    {
        if(!this.frozenHandlers.TryGetValue(id, out PacketHandler<TContext>? handler))
            return PacketHandlingStatus.InvalidId;

        // Console.WriteLine($"Handling {StateInfo.DirectedRegistry(Direction).Get(id)!.Packet}");
        return handler!.TryHandle(payload, connection, ctx) ? PacketHandlingStatus.Ok : PacketHandlingStatus.InvalidData;
    }

    protected void Register<TPacket>(PacketHandler<TContext, TPacket> handler)
        where TPacket : struct, IPacket<TPacket>
    {
        if(TPacket.PacketDirection != Direction)
            throw new InvalidOperationException($"Cannot add a handler for packet '{TPacket.PacketLocation}' (a.k.a: '{typeof(TPacket).Name}') because its direction is {TPacket.PacketDirection} and the state handler direction is '{Direction}'");

        FrozenRegistry<IPacketInfo> packetRegistry = StateInfo.DirectedRegistry(TPacket.PacketDirection);
        int packetId = packetRegistry.GetId(TPacket.PacketLocation);
        this.handlers!.Add(packetId, handler);
    }

    protected void Freeze()
    {
        if(this.handlers == null)
            throw new InvalidOperationException($"Cannot freeze twice packet handlers!");

        this.frozenHandlers = this.handlers.ToFrozenDictionary();
        this.handlers = null;
    }
}

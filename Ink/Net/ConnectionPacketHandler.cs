using System.Collections.Frozen;
using System.Diagnostics.CodeAnalysis;

namespace Ink.Net;

public sealed class ConnectionPacketHandler<TContext>(FrozenDictionary<NetworkState, PacketStateHandler<TContext>> handlers)
{
    private readonly FrozenDictionary<NetworkState, PacketStateHandler<TContext>> handlers = handlers;

    public bool TryGet(NetworkState state, [NotNullWhen(true)] out PacketStateHandler<TContext>? value)
        => this.handlers.TryGetValue(state, out value);

    public sealed class Builder
    {
        private readonly Dictionary<NetworkState, PacketStateHandler<TContext>> handlers = [];

        public Builder Register(NetworkState state, PacketStateHandler<TContext> handler)
        {
            this.handlers.Add(state, handler);
            return this;
        }

        public ConnectionPacketHandler<TContext> Build()
            => new(this.handlers.ToFrozenDictionary());
    }
}

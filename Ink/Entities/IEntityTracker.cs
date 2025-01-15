using Ink.Net;

namespace Ink.Entities;

public interface IEntityTracker : ITickable
{
    public static readonly IEntityTracker Null = new NullEntityTracker();

    void Remove();

    void Send<TPacket>(in TPacket packet)
        where TPacket : struct, IPacket<TPacket>;

    private sealed class NullEntityTracker : IEntityTracker
    {
        public void Tick()
        {
        }

        public void Remove()
        {
        }

        public void Send<TPacket>(in TPacket packet)
            where TPacket : struct, IPacket<TPacket>
        {
        }
    }
}

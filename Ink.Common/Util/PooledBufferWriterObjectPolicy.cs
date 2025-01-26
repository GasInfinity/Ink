using System.Buffers;
using Microsoft.Extensions.ObjectPool;
using Rena.Native.Buffers;

public sealed class PooledBufferWriterObjectPolicy<TArrayPool> : PooledObjectPolicy<PooledArrayBufferWriter<byte>>
        where TArrayPool : ArrayPool<byte>
{
    private readonly ArrayPool<byte> pool;

    public PooledBufferWriterObjectPolicy(TArrayPool pool) => this.pool = pool;

    public override PooledArrayBufferWriter<byte> Create()
        => new PooledArrayBufferWriter<byte>(this.pool);

    public override bool Return(PooledArrayBufferWriter<byte> obj)
    {
        obj.Reset();
        return true;
    }
}

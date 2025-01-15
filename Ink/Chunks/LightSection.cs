using Ink.Util.Extensions;
using System.Buffers;

namespace Ink.Chunks;

public struct LightSection : IDisposable
{
    public const int Size = Section.BlockCount / 2;

    private byte[] skyLight;
    private byte[] blockLight;

    public readonly bool HasSkylight
        => (this.skyLight?.Length ?? 0) != 0;

    public readonly bool HasBlocklight
        => (this.blockLight?.Length ?? 0) != 0;

    public LightSection()
    {
        this.skyLight = [];
        this.blockLight = [];
    }

    public bool TryWriteSkylight(IBufferWriter<byte> writer)
    {
        if (!HasSkylight)
            return false;

        writer.WriteVarInteger(this.skyLight.Length);
        writer.Write(this.skyLight);
        return true;
    }

    public bool TryWriteBlocklight(IBufferWriter<byte> writer)
    {
        if (!HasBlocklight)
            return false;

        writer.WriteVarInteger(this.blockLight.Length);
        writer.Write(this.blockLight);
        return true;
    }

    public void Dispose()
    {
        this = default;
    }
}

using System.Buffers;
using Ink.Registries;
using Ink.Util.Extensions;
using Rena.Native.Buffers.Extensions;

namespace Ink.Net.Structures;

public readonly record struct SoundEvent(Identifier SoundName, float? FixedRange)
{
    public readonly Identifier SoundName = SoundName;
    public readonly float? FixedRange = FixedRange;

    public void Write(IBufferWriter<byte> writer)
    {
        writer.WriteJUtf8String(SoundName.ToString());

        if(writer.TryWriteOptional(FixedRange != null))
            writer.WriteSingle(FixedRange!.Value, false);
    }

    public static bool TryRead(ReadOnlySpan<byte> data, out int bytesRead, out SoundEvent value)
    {
        value = default;
        bytesRead = default;
        return false;
    }
}

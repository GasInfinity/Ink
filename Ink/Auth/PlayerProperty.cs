using System.Buffers;
using Ink.Util.Extensions;

namespace Ink.Auth;

public readonly record struct PlayerProperty(string Name, string Value, string? Signature)
{
    public readonly string Name = Name;
    public readonly string Value = Value;
    public readonly string? Signature = Signature;
    
    public void Write(IBufferWriter<byte> writer)
    {
        writer.WriteJUtf8String(Name);
        writer.WriteJUtf8String(Value);

        if(writer.TryWriteOptional(Signature != null))
            writer.WriteJUtf8String(Signature);
    }

    public static bool TryRead(ReadOnlySpan<byte> data, out int bytesRead, out PlayerProperty value)
    {
        value = default;
        bytesRead = default;
        return false;
    }
}

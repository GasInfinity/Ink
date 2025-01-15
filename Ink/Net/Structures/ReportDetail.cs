using System.Buffers;
using Ink.Util.Extensions;

namespace Ink.Net.Structures;

public readonly record struct ReportDetail(string Title, string Description)
{
    public readonly string Title = Title;
    public readonly string Description = Description;
    
    public void Write(IBufferWriter<byte> writer)
    {
        writer.WriteJUtf8String(Title);
        writer.WriteJUtf8String(Description);
    }

    public static bool TryRead(ReadOnlySpan<byte> data, out int bytesRead, out ReportDetail value)
    {
        value = default;
        bytesRead = default;
        return false;
    }
}

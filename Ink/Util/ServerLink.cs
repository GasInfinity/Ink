using System.Buffers;
using Ink.Text;
using Ink.Util.Extensions;
using Rena.Native.Buffers.Extensions;

namespace Ink.Util;

public readonly record struct ServerLink(ServerLink.Kind LinkKind, TextPart Label, string Url)
{
    public readonly Kind LinkKind = LinkKind;
    public readonly TextPart Label = Label;
    public readonly string Url = Url;

    public enum Kind
    {
        Custom = -1,
        BugReport = 0,
        CommunityGuidelines,
        Support,
        Status,
        Feedback,
        Community,
        Website,
        Forums,
        News,
        Announcements,
    }

    public ServerLink(TextPart label, string url) : this(Kind.Custom, label, url)
    { }

    public ServerLink(Kind kind, string url) : this(kind, TextPart.Empty(), url)
    { }

    public void Write(IBufferWriter<byte> writer)
    {
        writer.WriteRaw(LinkKind != Kind.Custom);

        if(LinkKind == Kind.Custom)
            writer.WriteNbt(Label, InkNbtContext.TextPart);

        writer.WriteJUtf8String(Url);
    }

    public static bool TryRead(ReadOnlySpan<byte> data, out int bytesRead, out ServerLink value)
    {
        value = default;
        bytesRead = default;
        return false;
    }
}

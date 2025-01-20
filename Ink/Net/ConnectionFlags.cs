using NetEscapades.EnumGenerators;

namespace Ink.Net;

[Flags]
[EnumExtensions]
public enum ConnectionFlags : byte
{
    None,
    Compressed = 1 << 0,
    Encrypted = 1 << 1
}

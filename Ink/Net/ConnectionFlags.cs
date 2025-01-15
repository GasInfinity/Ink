namespace Ink.Net;

[Flags]
public enum ConnectionFlags : byte
{
    None,
    Compressed = 1 << 0,
    Encrypted = 1 << 1
}
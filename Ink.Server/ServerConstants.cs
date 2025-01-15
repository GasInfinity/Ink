using Ink.Net;

namespace Ink.Server;

public static class ServerConstants
{
    public const MinecraftProtocol ServerProtocol = MinecraftProtocol.V1_21_4;
    public const int RsaKeypairSize = 1024;
    public const int ConnectionTokenLength = 16;
    public const int TicksBetweenKeepAlives = 8 * 20;
    public const int TicksKeepAliveTimeout = 15 * 20;

    public static readonly byte[] ServerBrand = "\x3ink"u8.ToArray();
}

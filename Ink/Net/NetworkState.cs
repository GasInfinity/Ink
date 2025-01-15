using NetEscapades.EnumGenerators;

namespace Ink.Net;

[EnumExtensions]
public enum NetworkState
{
    Handshake,
    Status = 1,
    Login = 2,
    Transfer = 3,
    Configuration,
    Play
}

using NetEscapades.EnumGenerators;

namespace Ink.SourceGenerator.Packet;

/// <summary>See NetworkDirection inside Ink for more info</summary>
[EnumExtensions]
public enum PacketSide
{
    Clientbound,
    Serverbound
}

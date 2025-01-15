using System.Text.Json.Serialization;
using Ink.SourceGenerator.Converters.Json;

namespace Ink.SourceGenerator.Packet;

[JsonConverter(typeof(LowerSnakeEnumJsonConverter<PacketKind>))]
public enum PacketKind
{
    Common,
    Handshake,
    Status,
    Login,
    Configuration,
    Play
}

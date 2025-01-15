
namespace Ink.SourceGenerator.Packet;

public enum PacketArgumentType
{
    UInt8,
    Int8,
    UInt16,
    Int16,
    UInt32,
    Int32,
    UInt64,
    Int64,
    Single,
    Double,
    Bool,
    String,
    TextComponent,
    JsonTextComponent,
    VarInt,
    VarLong,
    Identifier,
    Uuid,
    ServerStatus,
    Angle,
    Int16Velocity,
    BlockPosition,
    RegistryEntry,
    KnownPack,
    TagEntry,
    ReportDetail,
    ServerLink,
    PlayerProperty,
    ExplosionRecordOffset,

    BoundedArray,
    UnboundedByteArray,
    Optional
}

public static class PacketArgumentTypes
{
    public static string GetTypeName(this PacketArgumentType type, object? extra = null, string? @if = null)
        => (type switch
        {
            PacketArgumentType.UInt8 => extra?.ToString() ?? "byte",
            PacketArgumentType.Int8 => extra?.ToString() ?? "sbyte",
            PacketArgumentType.UInt16 => "ushort",
            PacketArgumentType.Int16 => "short",
            PacketArgumentType.UInt32 => "uint",
            PacketArgumentType.Int32 => "int",
            PacketArgumentType.UInt64 => "ulong",
            PacketArgumentType.Int64 => "long",
            PacketArgumentType.Single => "float",
            PacketArgumentType.Double => "double",
            PacketArgumentType.Bool => "bool",
            PacketArgumentType.String => "string",
            PacketArgumentType.TextComponent or PacketArgumentType.JsonTextComponent => "ChatPart",
            PacketArgumentType.VarInt => extra?.ToString() ?? "int",
            PacketArgumentType.VarLong => extra?.ToString() ?? "long",
            PacketArgumentType.Identifier => "Identifier",
            PacketArgumentType.Uuid => "Uuid",
            PacketArgumentType.ServerStatus => "ServerStatus",
            PacketArgumentType.Angle => "Angle",
            PacketArgumentType.Int16Velocity => "Int16Velocity",
            PacketArgumentType.BlockPosition => "BlockPosition",
            PacketArgumentType.RegistryEntry => "RegistryEntry",
            PacketArgumentType.KnownPack => "KnownPack",
            PacketArgumentType.TagEntry => "TagEntry",
            PacketArgumentType.ReportDetail => "ReportDetail",
            PacketArgumentType.ServerLink => "ServerLink",
            PacketArgumentType.PlayerProperty => "PlayerProperty",
            PacketArgumentType.ExplosionRecordOffset => "(byte, byte, byte)",

            PacketArgumentType.Optional => $"{Enum.Parse<PacketArgumentType>(extra!.ToString()!).GetTypeName(null)}?",
            PacketArgumentType.BoundedArray => $"ImmutableArray<{Enum.Parse<PacketArgumentType>(extra!.ToString()!).GetTypeName(null)}>",
            PacketArgumentType.UnboundedByteArray => "ImmutableArray<byte>",
            _ => throw new InvalidDataException()
        }) + (type != PacketArgumentType.Optional && !string.IsNullOrEmpty(@if) ? "?" : string.Empty);
}

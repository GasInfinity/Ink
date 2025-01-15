using Ink.SourceGenerator.Util;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using NetEscapades.EnumGenerators;

namespace Ink.SourceGenerator.Packet;

public sealed class KnownCompoundFieldType(KnownCompoundFieldType.CompoundKind Kind) : IPacketFieldType
{
    [EnumExtensions]
    public enum CompoundKind
    {
        Uuid,
        Identifier,
        VersionedIdentifier,
        ServerStatus,
        [Display(Name = "TextPart")] JsonTextPart,
        [Display(Name = "TextPart")] TextPart,
        BlockPosition,
        SectionPosition,
        GameProfile,
        SyncedRegistry,
        RegistryTags,
        ReportDetail,
        ServerLink,
        Statistic,
        NbtTag,
        ParticleData,
        PlayersInfo,
        EquipmentData,
        ChatArgumentSignature,
        SoundEvent,
        ScoreboardData,
        ChunkData,
        LightData,
        ItemStack
    }

    public readonly CompoundKind Kind = Kind;

    public void AppendTypename(IndentingStringBuilder writer)
    {
        writer.Write(Kind.ToStringFast());
    }

    public void AppendWriting(IndentingStringBuilder writer, string fieldName)
    {
        switch(Kind)
        {
            case CompoundKind.JsonTextPart:
            case CompoundKind.ServerStatus:
                {
                    writer.WriteLine($"writer.WriteJsonJUtf8({fieldName}, InkJsonContext.Default.{Kind.ToStringFast()});");
                    break;
                }
            case CompoundKind.TextPart:
            case CompoundKind.NbtTag:
                {
                    writer.WriteLine($"writer.WriteNbt({fieldName}, InkNbtContext.{Kind.ToStringFast()});");
                    break;
                }
            default:
                {
                    writer.WriteLine($"{fieldName}.Write(writer);");
                    break;
                }
        }
    }

    public void AppendReading(IndentingStringBuilder writer, string fieldName)
    {
        switch(Kind)
        {
            case CompoundKind.JsonTextPart:
            case CompoundKind.ServerStatus:
                {
                    writer.WriteLine(true, $$"""
                            if (VarInteger<uint>.TryDecode(payload, out int bytesRead{{fieldName}}, out uint length{{fieldName}}) != OperationStatus.Done)
                            {
                                result = default;
                                return false;
                            }

                            payload = payload[bytesRead{{fieldName}}..];
                            if(payload.Length < length{{fieldName}})
                            {
                                result = default;
                                return false;
                            }

                            {{fieldName}} = JsonSerializer.Deserialize(payload[..(int)length{{fieldName}}], InkJsonContext.Default.{{Kind.ToStringFast()}});
                            payload = payload[(int)length{{fieldName}}..];
                            """);
                    break;
                }
            case CompoundKind.NbtTag:
            case CompoundKind.TextPart:
                {
                    writer.WriteLine($"// read ({fieldName}, InkNbtContext.{Kind.ToStringFast()});");
                    writer.WriteLine($"{fieldName} = default;");
                    break;
                }
            default:
                {
                    writer.WriteLine(true, $$"""
                            if(!{{Kind.ToStringFast()}}.TryRead(payload, out int bytesRead{{fieldName}}, out {{Kind.ToStringFast()}} result{{fieldName}}))
                            {
                                result = default;
                                return false;
                            }

                            {{fieldName}} = result{{fieldName}};
                            payload = payload[bytesRead{{fieldName}}..];
                            """);
                    break;
                }
        }
    }

    public static KnownCompoundFieldType Parse(ReadOnlySpan<char> typeDescription)
    {
        if(!CompoundKindExtensions.TryParse(typeDescription, out CompoundKind kind))
            throw new UnreachableException($"We shouldn't be here, as {typeDescription} is unknown");

        return new KnownCompoundFieldType(kind);
    }
}

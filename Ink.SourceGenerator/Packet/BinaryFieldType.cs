using Ink.SourceGenerator.Util;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;
using NetEscapades.EnumGenerators;

namespace Ink.SourceGenerator.Packet;

public sealed class BinaryFieldType(BinaryFieldType.Kind BinaryKind, string? Alias = null) : IPacketFieldType
{
    [EnumExtensions]
    public enum Kind
    {
        [Display(Name = "bool")] Bool,
        [Display(Name = "byte")] UInt8,
        [Display(Name = "sbyte")] Int8,
        [Display(Name = "ushort")] UInt16,
        [Display(Name = "short")] Int16,
        [Display(Name = "uint")] UInt32,
        [Display(Name = "int")] Int32,
        [Display(Name = "ulong")] UInt64,
        [Display(Name = "long")] Int64,
        [Display(Name = "float")] Single,
        [Display(Name = "double")] Double,
        [Display(Name = "int")] VarInt,
        [Display(Name = "long")] VarLong
    }

    public readonly Kind BinaryKind = BinaryKind;
    public readonly string Alias = Alias ?? string.Empty;

    public void AppendTypename(IndentingStringBuilder writer)
        => writer.Write(string.IsNullOrEmpty(Alias) ? BinaryKind.ToStringFast() : Alias);

    public void AppendWriting(IndentingStringBuilder writer, string fieldName)
    {
        switch(BinaryKind)
        {
            case Kind.UInt8:
            case Kind.Int8:
            case Kind.Bool:
                {
                    writer.WriteLine($"writer.WriteRaw({(string.IsNullOrEmpty(Alias) ? string.Empty : $"({BinaryKind.ToStringFast()})")}{fieldName});");
                    break;
                }
            case Kind.UInt16:
            case Kind.Int16:
            case Kind.UInt32:
            case Kind.Int32:
            case Kind.UInt64:
            case Kind.Int64:
            case Kind.Single:
            case Kind.Double:
                {
                    writer.WriteLine($"writer.Write{BinaryKind}({(string.IsNullOrEmpty(Alias) ? string.Empty : $"({BinaryKind.ToStringFast()})")}{fieldName}, false);");
                    break;
                }
            case Kind.VarInt:
                {
                    writer.WriteLine($"writer.WriteVarInteger({(string.IsNullOrEmpty(Alias) ? string.Empty : "(int)")}{fieldName});");
                    break;
                }
            case Kind.VarLong:
                {
                    writer.WriteLine($"writer.WriteVarLong({(string.IsNullOrEmpty(Alias) ? string.Empty : "(long)")}{fieldName});");
                    break;
                }
        }
    }

    public void AppendReading(IndentingStringBuilder writer, string fieldName)
    {
        switch(BinaryKind)
        {
            case Kind.UInt8:
            case Kind.Int8:
            case Kind.Bool:
            case Kind.UInt16:
            case Kind.Int16:
            case Kind.UInt32:
            case Kind.Int32:
            case Kind.UInt64:
            case Kind.Int64:
            case Kind.Single:
            case Kind.Double:
                {
                    writer.WriteLine(true, $$"""
                            if(payload.Length < sizeof({{BinaryKind.ToStringFast()}}))
                            {
                                result = default;
                                return false;
                            }
                            """);
                    switch(BinaryKind)
                    {
                        case Kind.UInt8:
                            {
                                writer.WriteLine($"{fieldName} = {(string.IsNullOrEmpty(Alias) ? string.Empty : $"({Alias})")}payload[0];");
                                break;
                            }
                        case Kind.Int8:
                        case Kind.Bool:
                            {
                                writer.WriteLine($"{fieldName} = {(string.IsNullOrEmpty(Alias) ? string.Empty : $"({Alias})")}Unsafe.BitCast<byte, {BinaryKind.ToStringFast()}>(payload[0]);");
                                break;
                            }
                        default:
                            {
                                writer.WriteLine($"{fieldName} = {(string.IsNullOrEmpty(Alias) ? string.Empty : $"({Alias})")}BinaryPrimitives.Read{BinaryKind}BigEndian(payload);");
                                break;
                            }
                    }
                    writer.WriteLine($"payload = payload[(sizeof({BinaryKind.ToStringFast()}))..];");
                    break;
                }
            case Kind.VarInt:
            case Kind.VarLong:
                {
                    writer.WriteLine(true, $$"""
                            if(VarInteger<u{{BinaryKind.ToStringFast()}}>.TryDecode(payload, out int bytesRead{{fieldName}}, out u{{BinaryKind.ToStringFast()}} result{{fieldName}}) != OperationStatus.Done)
                            {
                                result = default;
                                return false;
                            }

                            {{fieldName}} = ({{(string.IsNullOrEmpty(Alias) ? $"{BinaryKind.ToStringFast()}" : Alias)}})result{{fieldName}};
                            payload = payload[bytesRead{{fieldName}}..];
                            """);
                    break;
                }
        } 
    }

    public static bool TryParse(ReadOnlySpan<char> typeDescription, [NotNullWhen(true)] out BinaryFieldType? value)
    {
        int parenStart = typeDescription.IndexOf('(');

        ReadOnlySpan<char> alias = ReadOnlySpan<char>.Empty;
        if(parenStart != -1)
        {
            alias = typeDescription[(parenStart + 1)..typeDescription.LastIndexOf(')')];
            typeDescription = typeDescription.Slice(0, parenStart).Trim();
        }

        if(!Enum.TryParse<Kind>(typeDescription, out Kind kind))
        {
            value = null;
            return false;
        }
        
        value = new BinaryFieldType(kind, alias.ToString());
        return true;
    }
}

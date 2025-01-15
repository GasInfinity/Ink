using Ink.SourceGenerator.Util;
using System.Diagnostics.CodeAnalysis;

namespace Ink.SourceGenerator.Packet;

public sealed class IdFieldType(IPacketFieldType Type) : IPacketFieldType
{
    public readonly IPacketFieldType Type = Type;

    public void AppendTypename(IndentingStringBuilder writer)
    {
        writer.Write("IdOr<");
        Type.AppendTypename(writer);
        writer.Write(">");
    }

    public void AppendWriting(IndentingStringBuilder writer, string fieldName)
    {
        writer.WriteLine($"writer.WriteVarInteger({fieldName}.Id);");
        writer.WriteLine($"if({fieldName}.HasValue)");

        using(writer.EnterBlock())
            Type.AppendWriting(writer, $"({fieldName}.Value)");
    }

    public void AppendReading(IndentingStringBuilder writer, string fieldName)
    {
        writer.WriteLine($$"""
                if(VarInteger<uint>.TryDecode(payload, out int bytesRead{{fieldName}}, out uint id{{fieldName}}) != OperationStatus.Done)
                {
                    result = default;
                    return false;
                }
                payload = payload[bytesRead{{fieldName}}..];

                if(id{{fieldName}} != 0)
                {
                    {{fieldName}} = new((int)id{{fieldName}});
                }
                else
                """);
        using(writer.EnterBlock())
        {
            Type.AppendTypename(writer);
            writer.WriteLine($" value{fieldName};");
            Type.AppendReading(writer, $"value{fieldName}");
            writer.WriteLine($"{fieldName} = new(value{fieldName});");
        }
    }

    public static bool TryParse(ReadOnlySpan<char> typeDescription, [NotNullWhen(true)] out IdFieldType? value)
    {
        if(!typeDescription.StartsWith("Id"))
        {
            value = null;
            return false;
        }

        int parenStart = typeDescription.IndexOf("(");

        if(parenStart == -1)
        {
            value = null;
            return false;
        }

        IPacketFieldType inner = IPacketFieldType.Parse(typeDescription[(parenStart + 1)..typeDescription.LastIndexOf(")")]);
        value = new IdFieldType(inner);
        return true;
    }
}

using Ink.SourceGenerator.Util;

namespace Ink.SourceGenerator.Packet;

public sealed class OptionalFieldType(IPacketFieldType Type) : IPacketFieldType
{
    public readonly IPacketFieldType Type = Type;

    public void AppendTypename(IndentingStringBuilder writer)
    {
        Type.AppendTypename(writer);
        writer.Write("?");
    }

    public void AppendWriting(IndentingStringBuilder writer, string fieldName)
    {
        writer.Write($"if({fieldName} is ");
        Type.AppendTypename(writer);
        writer.WriteLine($" {fieldName}NotNull)");
        using (writer.EnterBlock())
        {
            writer.WriteLine($"writer.WriteRaw(true);");
            Type.AppendWriting(writer, $"{fieldName}NotNull");
        }
        writer.WriteLine(true, $$"""
                else
                {
                    writer.WriteRaw(false);
                }
                """);
    }

    public void AppendReading(IndentingStringBuilder writer, string fieldName)
    {
        writer.WriteLine(true, $$"""
                if(payload.Length < sizeof(byte))
                {
                    result = default;
                    return false;
                }

                bool present{{fieldName}} = payload[0] != 0;
                payload = payload[1..];

                if(present{{fieldName}})
                """);

        using(writer.EnterBlock())
        {
            Type.AppendReading(writer, fieldName);
        }

        writer.WriteLine(true, $$"""
                else
                {
                    {{fieldName}} = null;
                }
                """);
    }
}

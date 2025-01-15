using System.Diagnostics.CodeAnalysis;
using Ink.SourceGenerator.Util;

namespace Ink.SourceGenerator.Packet;

public sealed class StringFieldType(int MaxLength) : IPacketFieldType
{
    public readonly int MaxLength = MaxLength;

    public void AppendTypename(IndentingStringBuilder writer)
        => writer.Write("string");

    public void AppendWriting(IndentingStringBuilder writer, string fieldName)
    {
        if(MaxLength > 0)
        {
            // Put logic to throw exception or smth
        }

        writer.WriteLine($"writer.WriteJUtf8String({fieldName});");
    }

    public void AppendReading(IndentingStringBuilder writer, string fieldName)
    {
        writer.WriteLine(true, $$"""
                if(JUtf8String.TryDecode(payload, out int bytesRead{{fieldName}}, out {{fieldName}}) != OperationStatus.Done)
                {
                    result = default;
                    return false;
                }

                payload = payload[bytesRead{{fieldName}}..];
                """);
    }

    public static bool TryParse(ReadOnlySpan<char> typeDescription, [NotNullWhen(true)] out StringFieldType? value)
    {
        if(!typeDescription.StartsWith("String"))
        {
            value = null;
            return false;
        }

        int parenStart = typeDescription.IndexOf("(");
        int maxLength = parenStart == -1 ? -1 : int.Parse(typeDescription[(parenStart + 1)..typeDescription.LastIndexOf(")")]);
        value = new StringFieldType(maxLength);
        return true;
    }
}

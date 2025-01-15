using System.Text.Json.Serialization;
using Ink.SourceGenerator.Converters.Json;
using Ink.SourceGenerator.Util;

namespace Ink.SourceGenerator.Packet;

[JsonConverter(typeof(PacketFieldTypeJsonConverter))]
public interface IPacketFieldType
{
    void AppendTypename(IndentingStringBuilder writer);
    void AppendWriting(IndentingStringBuilder writer, string fieldName);
    void AppendReading(IndentingStringBuilder writer, string fieldName);

    public static IPacketFieldType Parse(ReadOnlySpan<char> value)
    {
        value = value.Trim();

        if(value.EndsWith('?'))
            return new OptionalFieldType(Parse(value[..^1]));

        int conditionalStart = value.LastIndexOf('{');

        if(conditionalStart != -1)
            return ConditionalFieldType.Parse(Parse(value.Slice(0, conditionalStart)), value.Slice(conditionalStart));

        int arrayStart = value.IndexOf('[');

        if(arrayStart != -1)
           return ArrayFieldType.Parse(Parse(value.Slice(0, arrayStart)), value.Slice(arrayStart));

        if(IdFieldType.TryParse(value, out IdFieldType? idField))
            return idField;

        if(StringFieldType.TryParse(value, out StringFieldType? strField))
            return strField;

        if(BinaryFieldType.TryParse(value, out BinaryFieldType? binField))
            return binField;

        return KnownCompoundFieldType.Parse(value);
    }
}

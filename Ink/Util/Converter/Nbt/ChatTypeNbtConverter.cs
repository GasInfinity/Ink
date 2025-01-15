using System.Collections.Immutable;
using Ink.Chat;
using Ink.Nbt;
using Ink.Nbt.Serialization;

namespace Ink.Util.Converter.Nbt;

public sealed class ChatTypeNbtConverter : NbtConverter<ChatType>
{
    public static readonly ChatTypeNbtConverter Shared = new();

    private ChatTypeNbtConverter()
    {
    }

    public override ChatType Read<TDatatypeReader>(ref NbtReader<TDatatypeReader> reader)
    {
        throw new NotImplementedException();
    }

    public override void Write<TDatatypeWriter>(NbtWriter<TDatatypeWriter> writer, ChatType value)
    {
        writer.WriteCompoundStart();
        writer.WriteProperty(NbtTagType.Compound, "chat");
        WriteContent(writer, value.Chat);
        writer.WriteProperty(NbtTagType.Compound, "narration");
        WriteContent(writer, value.Narration);
        writer.WriteCompoundEnd();
    }

    private static void WriteContent<TDatatypeWriter>(NbtWriter<TDatatypeWriter> writer, in ChatType.Content content)
        where TDatatypeWriter : struct, INbtDatatypeWriter<TDatatypeWriter>
    {
        writer.WriteCompoundStart();
        writer.WriteString("translation_key", content.TranslationKey);
        ImmutableArray<string> contentParameters = content.Parameters;

        if (!contentParameters.IsDefaultOrEmpty)
        {
            writer.WriteListStart("parameters", NbtTagType.String, contentParameters.Length);
            foreach (string parameter in contentParameters)
                writer.WriteString(parameter);
            writer.WriteListEnd();
        }
        writer.WriteCompoundEnd();
    }
}

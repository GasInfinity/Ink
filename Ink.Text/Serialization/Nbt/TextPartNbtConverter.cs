using Ink.Text.Content;
using Ink.Nbt;
using Ink.Nbt.Serialization;

namespace Ink.Text.Serialization.Nbt;

public sealed class TextPartNbtConverter : NbtConverter<TextPart>
{
    public static readonly TextPartNbtConverter Shared = new();

    public override TextPart Read<TDatatypeReader>(ref NbtReader<TDatatypeReader> reader)
    {
        throw new NotImplementedException();
    }

    public override void Write<TDatatypeWriter>(NbtWriter<TDatatypeWriter> writer, TextPart value)
    {
        if (value.Content is StringPartContent stringContent && value.Extra.IsDefaultOrEmpty && value.Style.IsEmpty)
        {
            writer.WriteString(stringContent.Content);
            return;
        }

        WriteOnlyCompound(writer, value);
    }

    private static void WriteOnlyCompound<TDatatypeWriter>(NbtWriter<TDatatypeWriter> writer, TextPart value)
        where TDatatypeWriter : struct, INbtDatatypeWriter<TDatatypeWriter>
    {
        writer.WriteCompoundStart();
        switch (value.Content)
        {
            case StringPartContent sPart:
                {
                    writer.WriteString("text", sPart.Content);
                    break;
                }
            case TranslationPartContent tPart:
                {
                    writer.WriteString("translate", tPart.Translate);

                    if (!tPart.With.IsDefaultOrEmpty)
                    {
                        writer.WriteListStart("with", NbtTagType.Compound, tPart.With.Length);
                        foreach (TextPart part in tPart.With)
                            WriteOnlyCompound(writer, part);
                        writer.WriteListEnd();
                    }
                    break;
                }
            case KeyPartContent kPart:
                {
                    writer.WriteString("keybind", kPart.Keybind);
                    break;
                }
            case SelectorPartContent sePart:
                {
                    writer.WriteString("selector", sePart.Selector);
                    break;
                }
            case ScorePartContent scPart:
                {
                    writer.WriteProperty(NbtTagType.Compound, "score");
                    //ScoreChatComponentJsonConverter.Shared.Write(writer, scPart.Score);
                    break;
                }
        }

        //ChatStyleJsonConverter<THoverSerializer>.WriteChatStyle(writer, value.Style);

        if (!value.Extra.IsDefaultOrEmpty)
        {
            writer.WriteListStart("extra", NbtTagType.Compound, value.Extra.Length);
            foreach (TextPart part in value.Extra)
                WriteOnlyCompound(writer, part);
            writer.WriteListEnd();
        }
        writer.WriteCompoundEnd();
    }

    private static void WriteChatStyle<TDatatypeWriter>(NbtWriter<TDatatypeWriter> writer, TextStyle style)
        where TDatatypeWriter : struct, INbtDatatypeWriter<TDatatypeWriter>
    {
        if (style.Bold is bool bold)
            writer.WriteSByte("bold", bold);

        if (style.Italic is bool italic)
            writer.WriteSByte("italic", italic);

        if (style.Underlined is bool underlined)
            writer.WriteSByte("underlined", underlined);

        if (style.Strikethrough is bool strikethrough)
            writer.WriteSByte("strikethrough", strikethrough);

        if (style.Obfuscated is bool obfuscated)
            writer.WriteSByte("obfuscated", obfuscated);

        if (style.Color is TextColor color)
            writer.WriteString("color", color.ToString());

        if (style.Font is string font)
            writer.WriteString("font", font);

        if (style.Insertion is string insertion)
            writer.WriteString("insertion", insertion);

        /*
        if (style.ClickEvent is ClickEvent ce)
        {
            writer.WritePropertyName(SerializationConstants.ClickEventKey);
            ClickEventJsonConverter.Shared.Write(writer, ce, options);
        }

        if (style.HoverEvent is HoverEvent he)
        {
            writer.WritePropertyName(SerializationConstants.HoverEventKey);
            THoverSerializer.Write(writer, he, options);
        }*/
    }
}

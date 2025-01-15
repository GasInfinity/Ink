using System.Diagnostics;
using System.Text.Json;
using System.Text.Json.Serialization;
using Ink.Text.Content;

namespace Ink.Text.Serialization.Json;

// TODO: This can be done better maybe?
public sealed class TextPartJsonConverter<THoverSerializer> : JsonConverter<TextPart>
    where THoverSerializer : IHoverEventJsonConverter<THoverSerializer>
{
    public static readonly TextPartJsonConverter<THoverSerializer> Shared = new();

    public override TextPart Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        while (reader.TokenType == JsonTokenType.Comment && reader.Read()) ;

        switch (reader.TokenType)
        {
            case JsonTokenType.StartObject:
                {
                    TextPartBuilder partBuilder = new TextPartBuilder();
                    TextStyleBuilder styleBuilder = new TextStyleBuilder();

                    while (reader.Read())
                    {
                        switch (reader.TokenType)
                        {
                            case JsonTokenType.PropertyName:
                                {
                                    if (TryReadPartContent(ref reader, partBuilder, options))
                                        break;

                                    if (TryReadTextStyle(ref reader, styleBuilder, options))
                                        break;

                                    if (reader.ValueTextEquals(SerializationConstants.ExtraKey))
                                    {
                                        reader.Read();

                                        if (reader.TokenType != JsonTokenType.StartArray)
                                            throw new JsonException($"Error while reading {nameof(TextPart)} 'extra', value is not an array");

                                        while (reader.Read())
                                        {
                                            switch (reader.TokenType)
                                            {
                                                case JsonTokenType.EndArray:
                                                    break;
                                                case JsonTokenType.Comment:
                                                    break;
                                                default:
                                                    {
                                                        partBuilder.Append(Read(ref reader, typeToConvert, options));
                                                        break;
                                                    }
                                            }
                                        }
                                    }
                                    break;
                                }
                            case JsonTokenType.EndObject:
                                return partBuilder.ToTextPart(); 
                            default:
                                throw new JsonException($"Invalid {nameof(JsonTokenType)} encountered while reading {nameof(TextPart)}");
                        }
                    }

                    throw new JsonException($"Error while reading {nameof(TextPart)}, {nameof(JsonTokenType.EndObject)} expected");
                }
            case JsonTokenType.StartArray:
                {
                    TextPartBuilder root = new TextPartBuilder();

                    while (reader.Read())
                    {
                        switch (reader.TokenType)
                        {
                            case JsonTokenType.EndArray:
                                return root.ToTextPart();
                            case JsonTokenType.Comment:
                                break;
                            default:
                                {
                                    if (object.ReferenceEquals(root.CurrentContent, StringPartContent.Empty))
                                    {
                                        root.CopyFrom(Read(ref reader, typeToConvert, options));
                                    }
                                    else
                                    {
                                        root.Append(Read(ref reader, typeToConvert, options));
                                    }
                                    break;
                                }
                        }
                    }

                    throw new JsonException($"Error while reading {nameof(TextPart)}, {nameof(JsonTokenType.EndArray)} expected");
                }
            case JsonTokenType.String:
                return TextPart.String(reader.GetString()!);
            case JsonTokenType.True or JsonTokenType.False:
                return TextPart.String(reader.GetBoolean()!.ToString());
            case JsonTokenType.Number:
                return TextPart.String(reader.GetDouble()!.ToString());
            case JsonTokenType.Null:
                return TextPart.String("<null>");
            default:
                throw new JsonException($"Invalid {nameof(JsonTokenType)} encountered while reading {nameof(TextPart)}");
        }

        throw new JsonException($"Cannot deserialize {nameof(TextPart)}, we got no data");
    }

    private static bool TryReadPartContent(ref Utf8JsonReader reader, TextPartBuilder builder, JsonSerializerOptions options)
    {
        if (reader.ValueTextEquals(SerializationConstants.TextKey))
        {
            reader.Read();
            builder.Content(new StringPartContent(reader.GetString()!));
        }
        else if (reader.ValueTextEquals(SerializationConstants.TranslateKey))
        {
            reader.Read();
            builder.Content(new TranslationPartContent(reader.GetString()!, builder.CurrentContent is TranslationPartContent t ? t.With : []));
        }
        else if (reader.ValueTextEquals(SerializationConstants.WithKey))
        {
            reader.Read();

            List<TextPart> with = [];
            if (reader.TokenType != JsonTokenType.StartArray)
                throw new JsonException($"Error while reading {nameof(TextPart)} 'with', value is not an array");

            while (reader.Read())
            {
                switch (reader.TokenType)
                {
                    case JsonTokenType.EndArray:
                        break;
                    case JsonTokenType.Comment:
                        break;
                    default:
                        {
                            with.Add(Shared.Read(ref reader, typeof(TextPart), options));
                            break;
                        }
                }
            }

            builder.Content(new TranslationPartContent(builder.CurrentContent is TranslationPartContent t ? t.Translate : string.Empty, [.. with]));
        }
        else if (reader.ValueTextEquals(SerializationConstants.KeybindKey))
        {
            reader.Read();
            builder.Content(new KeyPartContent(reader.GetString()!));
        }
        else if (reader.ValueTextEquals(SerializationConstants.ScoreKey))
        {
            reader.Read();
            builder.Content(new ScorePartContent(ScoreChatComponentJsonConverter.Shared.Read(ref reader, typeof(ScoreChatComponent), options)));
        }
        else if (reader.ValueTextEquals(SerializationConstants.SelectorKey))
        {
            reader.Read();
            builder.Content(new SelectorPartContent(reader.GetString()!));
        }
        else
        {
            return false;
        }

        return true;
    }

    public override void Write(Utf8JsonWriter writer, TextPart value, JsonSerializerOptions options)
    {
        if (value.Content is StringPartContent stringContent && value.Extra.IsDefaultOrEmpty && value.Style.IsEmpty)
        {
            writer.WriteStringValue(stringContent.Content);
            return;
        }

        writer.WriteStartObject();
        switch (value.Content)
        {
            case StringPartContent sPart:
                {
                    writer.WriteString(SerializationConstants.TextKey, sPart.Content);
                    break;
                }
            case TranslationPartContent tPart:
                {
                    writer.WriteString(SerializationConstants.TranslateKey, tPart.Translate);

                    if (!tPart.With.IsDefaultOrEmpty)
                    {
                        writer.WriteStartArray(SerializationConstants.WithKey);
                        foreach (TextPart part in tPart.With)
                            Write(writer, part, options);
                        writer.WriteEndArray();
                    }
                    break;
                }
            case KeyPartContent kPart:
                {
                    writer.WriteString(SerializationConstants.KeybindKey, kPart.Keybind);
                    break;
                }
            case SelectorPartContent sePart:
                {
                    writer.WriteString(SerializationConstants.SelectorKey, sePart.Selector);
                    break;
                }
            case ScorePartContent scPart:
                {
                    writer.WritePropertyName(SerializationConstants.ScoreKey);
                    ScoreChatComponentJsonConverter.Shared.Write(writer, scPart.Score, options);
                    break;
                }
            default: throw new UnreachableException();
        }

        WriteChatStyle(writer, value.Style, options);

        if (!value.Extra.IsDefaultOrEmpty)
        {
            writer.WriteStartArray(SerializationConstants.ExtraKey);
            foreach (TextPart part in value.Extra)
                Write(writer, part, options);
            writer.WriteEndArray();
        }
        writer.WriteEndObject();
    }

    private static bool TryReadTextStyle(ref Utf8JsonReader reader, TextStyleBuilder builder, JsonSerializerOptions options)
    {
        if (reader.ValueTextEquals(SerializationConstants.BoldKey))
        {
            reader.Read();
        }
        else if (reader.ValueTextEquals(SerializationConstants.ItalicKey))
        {
            reader.Read();
        }
        else if (reader.ValueTextEquals(SerializationConstants.UnderlinedKey))
        {
            reader.Read();
        }
        else if (reader.ValueTextEquals(SerializationConstants.StrikethroughKey))
        {
            reader.Read();
        }
        else if (reader.ValueTextEquals(SerializationConstants.ObfuscatedKey))
        {
            reader.Read();
        }
        else if (reader.ValueTextEquals(SerializationConstants.ColorKey))
        {
            reader.Read();
        }
        else if (reader.ValueTextEquals(SerializationConstants.FontKey))
        {
            reader.Read();
        }
        else if (reader.ValueTextEquals(SerializationConstants.InsertionKey))
        {
            reader.Read();
        }
        else if (reader.ValueTextEquals(SerializationConstants.ClickEventKey))
        {
            reader.Read();
        }
        else if (reader.ValueTextEquals(SerializationConstants.HoverEventKey))
        {
            reader.Read();
        }
        else
        {
            return false;
        }

        return true;
    }

    private static void WriteChatStyle(Utf8JsonWriter writer, TextStyle style, JsonSerializerOptions options)
    {
        if (style == null)
            return;

        if (style.Bold is bool bold)
            writer.WriteBoolean(SerializationConstants.BoldKey, bold);

        if (style.Italic is bool italic)
            writer.WriteBoolean(SerializationConstants.ItalicKey, italic);

        if (style.Underlined is bool underlined)
            writer.WriteBoolean(SerializationConstants.UnderlinedKey, underlined);

        if (style.Strikethrough is bool strikethrough)
            writer.WriteBoolean(SerializationConstants.StrikethroughKey, strikethrough);

        if (style.Obfuscated is bool obfuscated)
            writer.WriteBoolean(SerializationConstants.ObfuscatedKey, obfuscated);

        if (style.Color is TextColor color)
            writer.WriteString(SerializationConstants.ColorKey, color.ToString());

        if (style.Font is string font)
            writer.WriteString(SerializationConstants.FontKey, font);

        if (style.Insertion is string insertion)
            writer.WriteString(SerializationConstants.InsertionKey, insertion);

        if (style.ClickEvent is ClickEvent ce)
        {
            writer.WritePropertyName(SerializationConstants.ClickEventKey);
            ClickEventJsonConverter.Shared.Write(writer, ce, options);
        }

        if (style.HoverEvent is HoverEvent he)
        {
            writer.WritePropertyName(SerializationConstants.HoverEventKey);
            THoverSerializer.Write(writer, he, options);
        }
    }
}

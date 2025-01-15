using System.Text.Json;

namespace Ink.Text.Serialization.Json;

public struct LegacyHoverJsonConverter : IHoverEventJsonConverter<LegacyHoverJsonConverter>
{
    public static HoverEvent Read(ref Utf8JsonReader reader, JsonSerializerOptions options)
    {
        while (reader.TokenType == JsonTokenType.Comment && reader.Read()) ;

        Span<byte> tmpActionBuffer = stackalloc byte[12];

        switch (reader.TokenType)
        {
            case JsonTokenType.StartObject:
                {
                    HoverEventAction action = HoverEventAction.ShowText;
                    IHoverEventContent contents = TextPart.Empty();

                    while (reader.Read())
                    {
                        switch (reader.TokenType)
                        {
                            case JsonTokenType.EndObject:
                                return new()
                                {
                                    Action = action,
                                    Contents = contents
                                };
                            case JsonTokenType.PropertyName:
                                {
                                    if (reader.ValueTextEquals(SerializationConstants.ActionKey))
                                    {
                                        reader.Read();
                                        int copiedBytes = reader.CopyString(tmpActionBuffer);
                                        action = HoverEventActionExtensions.FromJsonString(tmpActionBuffer[..copiedBytes]);
                                    }
                                    else if (reader.ValueTextEquals(SerializationConstants.ValueKey))
                                    {
                                        reader.Read();
                                        switch(action) // TODO: ShowItem, ShowEntity
                                        {
                                            case HoverEventAction.ShowText:
                                                {
                                                    contents = TextPartJsonConverter<LegacyHoverJsonConverter>.Shared.Read(ref reader, typeof(TextPart), options);
                                                    break;
                                                }
                                        }
                                    }
                                    break;
                                }
                        }
                    }

                    throw new JsonException($"Error while reading {nameof(HoverEvent)}, {nameof(JsonTokenType.EndObject)} expected");
                }
            default:
                throw new JsonException($"Error while reading {nameof(HoverEvent)}, {nameof(JsonTokenType.StartObject)} expected");
        }

        throw new JsonException($"Cannot deserialize {nameof(HoverEvent)}, we got no data");
    }

    public static void Write(Utf8JsonWriter writer, HoverEvent value, JsonSerializerOptions options)
    {
        writer.WriteStartObject();
        writer.WriteString(SerializationConstants.ActionKey, value.Action.ToJsonString());
        switch (value.Contents)
        {
            case TextPart cPart:
                {
                    writer.WritePropertyName(SerializationConstants.ValueKey);
                    TextPartJsonConverter<LegacyHoverJsonConverter>.Shared.Write(writer, cPart, JsonSerializerOptions.Default);
                    break;
                }
            case ItemInfo _:
                {
                    break; // TODO: Item info
                }
            case EntityInfo _:
                {
                    break; // TODO: Entity info
                }
        }
        writer.WriteEndObject();
    }
}

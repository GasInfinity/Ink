using System.Text.Json;
using System.Text.Json.Serialization;

namespace Ink.Text.Serialization.Json;

public sealed class ClickEventJsonConverter : JsonConverter<ClickEvent>
{
    public static readonly ClickEventJsonConverter Shared = new();

    public static ReadOnlySpan<byte> ActionKey => "action"u8;
    public static ReadOnlySpan<byte> ValueKey => "value"u8;

    public override ClickEvent Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        while (reader.TokenType == JsonTokenType.Comment && reader.Read()) ;

        Span<byte> tmpActionBuffer = stackalloc byte[12];

        switch (reader.TokenType)
        {
            case JsonTokenType.StartObject:
                {
                    ClickEventAction action = ClickEventAction.OpenUrl;
                    string value = string.Empty;

                    while (reader.Read())
                    {
                        switch (reader.TokenType)
                        {
                            case JsonTokenType.Comment:
                                break;
                            case JsonTokenType.EndObject:
                                return new()
                                {
                                    Action = action,
                                    Value = value
                                };
                            case JsonTokenType.PropertyName:
                                {
                                    if (reader.ValueTextEquals(ActionKey))
                                    {
                                        reader.Read();
                                        int copiedBytes = reader.CopyString(tmpActionBuffer);
                                        action = ClickEventActionExtensions.FromJsonString(tmpActionBuffer[..copiedBytes]);
                                    }
                                    else if (reader.ValueTextEquals(ValueKey))
                                    {
                                        value = reader.GetString()!;
                                    }
                                    break;
                                }
                        }
                    }

                    throw new JsonException($"Error while reading {nameof(ClickEvent)}, {nameof(JsonTokenType.EndObject)} expected");
                }
            default:
                throw new JsonException($"Error while reading {nameof(ClickEvent)}, {nameof(JsonTokenType.StartObject)} expected");
        }

        throw new JsonException($"Cannot deserialize {nameof(ClickEvent)}, we got no data");
    }

    public override void Write(Utf8JsonWriter writer, ClickEvent value, JsonSerializerOptions options)
    {
        writer.WriteStartObject();
        writer.WriteString(ActionKey, value.Action.ToJsonString());
        writer.WriteString(ValueKey, value.Value);
        writer.WriteEndObject();
    }
}

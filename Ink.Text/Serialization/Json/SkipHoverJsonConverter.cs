using System.Text.Json;

namespace Ink.Text.Serialization.Json;

public struct SkipHoverJsonConverter : IHoverEventJsonConverter<SkipHoverJsonConverter>
{
    public static HoverEvent Read(ref Utf8JsonReader reader, JsonSerializerOptions options)
    {
        while (reader.TokenType == JsonTokenType.Comment && reader.Read()) ;

        switch (reader.TokenType)
        {
            case JsonTokenType.StartObject:
                {
                    reader.Skip();
                    return default;
                }
            default:
                throw new JsonException($"Error while reading {nameof(HoverEvent)}, {nameof(JsonTokenType.StartObject)} expected");
        }

        throw new JsonException($"Cannot deserialize {nameof(HoverEvent)}, we got no data");
    }

    public static void Write(Utf8JsonWriter writer, HoverEvent value, JsonSerializerOptions options)
    {
        writer.WriteStartObject();
        writer.WriteEndObject();
    }
}

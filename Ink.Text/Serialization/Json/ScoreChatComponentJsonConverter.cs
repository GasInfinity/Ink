using System.Text.Json;
using System.Text.Json.Serialization;

namespace Ink.Text.Serialization.Json;

public sealed class ScoreChatComponentJsonConverter : JsonConverter<ScoreChatComponent>
{
    public static readonly ScoreChatComponentJsonConverter Shared = new();

    public static ReadOnlySpan<byte> NameKey => "name"u8;
    public static ReadOnlySpan<byte> ObjectiveKey => "objective"u8;
    public static ReadOnlySpan<byte> ValueKey => "value"u8;

    public override ScoreChatComponent Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        while (reader.TokenType == JsonTokenType.Comment && reader.Read()) ;

        switch (reader.TokenType)
        {
            case JsonTokenType.StartObject:
                {
                    string name = string.Empty;
                    string objective = string.Empty;
                    string value = string.Empty;

                    while (reader.Read())
                    {
                        switch (reader.TokenType)
                        {
                            case JsonTokenType.Comment:
                                break;
                            case JsonTokenType.EndObject:
                                return new(name, objective, value);
                            case JsonTokenType.PropertyName:
                                {
                                    if (reader.ValueTextEquals(NameKey))
                                    {
                                        reader.Read();
                                        name = reader.GetString()!;
                                    }
                                    else if (reader.ValueTextEquals(ObjectiveKey))
                                    {
                                        reader.Read();
                                        objective = reader.GetString()!;
                                    }
                                    else if (reader.ValueTextEquals(ValueKey))
                                    {
                                        reader.Read();
                                        value = reader.GetString()!;
                                    }
                                    break;
                                }
                        }
                    }

                    throw new JsonException($"Error while reading {nameof(ScoreChatComponent)}, {nameof(JsonTokenType.EndObject)} expected");
                }
            default:
                throw new JsonException($"Error while reading {nameof(ScoreChatComponent)}, {nameof(JsonTokenType.StartObject)} expected");
        }

        throw new JsonException($"Cannot deserialize {nameof(ScoreChatComponent)}, we got no data");
    }

    public override void Write(Utf8JsonWriter writer, ScoreChatComponent value, JsonSerializerOptions options)
    {
        writer.WriteStartObject();
        writer.WriteString(NameKey, value.Name);
        writer.WriteString(ObjectiveKey, value.Objective);
        if (string.IsNullOrEmpty(value.Value))
            writer.WriteString(ValueKey, value.Value);
        writer.WriteEndObject();
    }
}

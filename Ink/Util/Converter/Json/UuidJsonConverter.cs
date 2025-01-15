using System.Text.Json;
using System.Text.Json.Serialization;

namespace Ink.Util.Converter.Json;

public sealed class UuidJsonConverter : JsonConverter<Uuid>
{
    public override Uuid Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        => Uuid.Parse(reader.GetString()!, null);

    public override void Write(Utf8JsonWriter writer, Uuid value, JsonSerializerOptions options)
        => writer.WriteStringValue(value.ToString());
}

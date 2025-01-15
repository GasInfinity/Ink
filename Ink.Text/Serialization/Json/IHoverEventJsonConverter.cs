using System.Text.Json;

namespace Ink.Text.Serialization.Json;

public interface IHoverEventJsonConverter<T>
    where T : IHoverEventJsonConverter<T>
{
    static abstract HoverEvent Read(ref Utf8JsonReader reader, JsonSerializerOptions options);
    static abstract void Write(Utf8JsonWriter writer, HoverEvent value, JsonSerializerOptions options);
}

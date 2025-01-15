using System.Text.Json;
using System.Text.Json.Serialization;

namespace Ink.SourceGenerator.Converters.Json;

internal class LowerSnakeEnumJsonConverter<TEnum> : JsonStringEnumConverter<TEnum>
    where TEnum : struct, Enum
{
    public LowerSnakeEnumJsonConverter() : base(JsonNamingPolicy.SnakeCaseLower, false)
    {}
}

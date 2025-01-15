using Ink.Text.Serialization.Json;
using System.Text.Json.Serialization;
using Ink.Text;
using Ink.Entities.Damage;
using Ink.Auth;

namespace Ink.Util;

[JsonSourceGenerationOptions(
    PropertyNamingPolicy = JsonKnownNamingPolicy.SnakeCaseLower,
    IncludeFields = true,
    Converters = [typeof(TextPartJsonConverter<SkipHoverJsonConverter>)] // TODO! ModernHover!
)]
[JsonSerializable(typeof(DamageType))]
[JsonSerializable(typeof(ServerStatus))]
[JsonSerializable(typeof(GameProfile))]
[JsonSerializable(typeof(TextPart))]
public sealed partial class InkJsonContext : JsonSerializerContext
{
}

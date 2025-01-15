using System.Text.Json.Serialization;

namespace Ink.Entities.Damage;

public sealed record DamageType
{
    [JsonPropertyName("message_id")]
    public string? MessageId { get; init; }
    [JsonPropertyName("exhaustion")]
    public required float Exhaustion { get; init; }
    [JsonPropertyName("scaling")]
    public required DamageScaling Scaling { get; init; }
    [JsonPropertyName("effects")]
    public DamageEffect Effects { get; init; }
    [JsonPropertyName("death_message_type")]
    public DeathMessageType DeathMessageType { get; init; }
}

using System.Collections.Immutable;
using System.Text.Json.Serialization;
using Ink.Text;
using Ink.Text.Serialization.Json;

namespace Ink.Util;

public readonly record struct ServerStatus
{
    public static readonly ServerStatus Default = new()
    {
        Version = new() { Name = "Ink", Protocol = -1 },
        Description = TextPart.String("An Ink Powered Minecraft Server")
    };

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public ServerVersion Version { get; init; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public ServerPlayers Players { get; init; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    [JsonConverter(typeof(TextPartJsonConverter<SkipHoverJsonConverter>))]
    public TextPart Description { get; init; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public string Favicon { get; init; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public bool EnforcesSecureChat { get; init; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public bool PreviewsChat { get; init; }
}

public readonly record struct ServerVersion
{
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public string Name { get; init; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public int Protocol { get; init; }
}

public readonly record struct ServerPlayers
{
    public int Max { get; init; }

    public int Online { get; init; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public ImmutableArray<ServerPlayer> Sample { get; init; }
}

public readonly record struct ServerPlayer
{
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public string Name { get; init; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public string Id { get; init; }
}

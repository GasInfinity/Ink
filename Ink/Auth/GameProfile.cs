using System.Buffers;
using System.Net;
using System.Net.Http.Json;
using System.Text.Json.Serialization;
using System.Web;
using Ink.Util;
using Ink.Util.Converter.Json;
using Ink.Util.Extensions;

namespace Ink.Auth;

[method: JsonConstructor]
public readonly record struct GameProfile(Uuid Id = default, string? Name = null, PlayerProperty[]? Properties = null)
{
    private static readonly Uri SessionServer = new Uri("https://sessionserver.mojang.com/");

    [JsonConverter(typeof(UuidJsonConverter))]
    public readonly Uuid Id = Id;
    public readonly string Name = Name ?? string.Empty;
    public readonly PlayerProperty[] Properties = Properties ?? Array.Empty<PlayerProperty>();

    public void Write(IBufferWriter<byte> writer)
    {
        Id.Write(writer);
        writer.WriteJUtf8String(Name);
        writer.WriteVarInteger(Properties.Length);

        for (int i = 0; i < Properties.Length; ++i)
            Properties[i].Write(writer);
    }

    public static bool TryRead(ReadOnlySpan<byte> payload, out int bytesRead, out GameProfile result)
    {
        result = default; 
        bytesRead = default; 
        return true;
    }

    public static async Task<GameProfile?> HasJoined(string username, string serverId, EndPoint? ip, CancellationToken cancellationToken)
    {
        using HttpClient client = new HttpClient() { BaseAddress = SessionServer };
        using HttpResponseMessage joinResponse = await client.GetAsync($"session/minecraft/hasJoined?username={HttpUtility.UrlEncode(username)}&serverId={HttpUtility.UrlEncode(serverId)}{(ip != null ? $"&ip={ip}" : string.Empty)}", cancellationToken);
        
        if(joinResponse.StatusCode == HttpStatusCode.OK)
            return await joinResponse.Content.ReadFromJsonAsync<GameProfile>(InkJsonContext.Default.GameProfile, cancellationToken);

        return null;
    }
}

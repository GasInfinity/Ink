using System.Buffers;
using Ink.Auth;
using Ink.Text;
using Ink.Util;
using Ink.Util.Extensions;
using Ink.Worlds;
using NetEscapades.EnumGenerators;
using Rena.Native.Buffers.Extensions;

namespace Ink.Net.Structures;

public readonly record struct PlayersInfo(PlayersInfo.Action Actions, PlayersInfo.Info[] Players)
{
    [Flags]
    [EnumExtensions]
    public enum Action : byte
    {
        AddPlayer = 1 << 0,
        InitializeChat = 1 << 1,
        UpdateGameMode = 1 << 2,
        UpdateListed = 1 << 3,
        UpdateLatency = 1 << 4,
        UpdateDisplayName = 1 << 5,
        UpdateListPriority = 1 << 6,
        UpdateHat = 1 << 7,
    }

    public readonly record struct Info(GameProfile Profile = default, GameMode GameMode = default, bool Listed = default, int Ping = default, TextPart? DisplayName = null)
    {
        public readonly GameProfile Profile = Profile;
        // TODO: Initialize Chat
        public readonly GameMode GameMode = GameMode;
        public readonly bool Listed = Listed;
        public readonly int Ping = Ping;
        public readonly TextPart? DisplayName = DisplayName;

        public void Write(IBufferWriter<byte> writer, Action actions)
        {
            Profile.Id.Write(writer);

            if(actions.HasFlagFast(Action.AddPlayer))
            {
                writer.WriteJUtf8String(Profile.Name);                

                writer.WriteVarInteger(Profile.Properties.Length);
                for(int i = 0; i < Profile.Properties.Length; ++i)
                    Profile.Properties[i].Write(writer);
            }

            if(actions.HasFlagFast(Action.UpdateGameMode))
            {
                writer.WriteVarInteger((int)GameMode);
            }

            if(actions.HasFlagFast(Action.UpdateListed))
            {
                writer.WriteRaw(Listed);
            }

            if(actions.HasFlagFast(Action.UpdateLatency))
            {
                writer.WriteVarInteger(Ping);
            }

            if(actions.HasFlagFast(Action.UpdateDisplayName))
            {
                if(writer.TryWriteOptional(DisplayName != null))
                {
                    writer.WriteNbt(DisplayName!.Value, InkNbtContext.TextPart);
                }
            }
        }

        // TODO: TryRead
    }

    public readonly Action Actions = Actions;
    public readonly Info[] Players = Players;

    public void Write(IBufferWriter<byte> writer)
    {
        writer.WriteRaw((byte)Actions);
        writer.WriteVarInteger(Players.Length);

        for(int i = 0; i < Players.Length; ++i)
            Players[i].Write(writer, Actions);
    }

    public static bool TryRead(ReadOnlySpan<byte> data, out int bytesRead, out PlayersInfo value)
    {
        // TODO: TryRead
        value = default;
        bytesRead = default;
        return false;
    }
}

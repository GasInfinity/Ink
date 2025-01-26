using Friflo.Engine.ECS;
using Ink.Auth;
using Ink.Worlds;

namespace Ink.Entities.Components;

public record struct EntityPlayerComponent(GameProfile Profile, PlayerSkinPart DisplayedSkin = default, PlayerMainHand MainHand = default, GameMode CurrentGameMode = default) : IComponent
{
    public readonly GameProfile Profile = Profile;
    public PlayerSkinPart DisplayedSkin = DisplayedSkin;
    public PlayerMainHand MainHand = MainHand;

    public GameMode CurrentGameMode = CurrentGameMode;
}

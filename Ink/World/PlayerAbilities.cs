namespace Ink.World;

public enum PlayerAbilities
{
    None,
    Invulnerable = 1 << 0,
    Flying = 1 << 1,
    AllowFlying = 1 << 2,
    CreativeMode = 1 << 3,
}

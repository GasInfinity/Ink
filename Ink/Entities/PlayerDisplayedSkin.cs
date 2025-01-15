namespace Ink.Entities;

[Flags]
public enum PlayerSkinPart : byte
{
    None = 0,
    Cape = 1 << 0,
    Jacket = 1 << 1,
    LeftSleeve = 1 << 2,
    RightSleeve = 1 << 3,
    LeftPantsLeg = 1 << 4,
    RightPantsLeg = 1 << 5,
    Hat = 1 << 6
}

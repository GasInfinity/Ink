namespace Ink.Entities;

[Flags]
public enum EntityMask : byte
{
    None,
    OnFire = 1 << 0,
    Crouching = 1 << 1,
    Unused = 1 << 2,
    Sprinting = 1 << 3,
    Swimming = 1 << 4,
    Invisible = 1 << 5,
    HasGlowingEffect = 1 << 6,
    FlyingWithElytra = 1 << 7,
}

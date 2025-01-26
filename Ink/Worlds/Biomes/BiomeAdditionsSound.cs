using Ink.Registries;

namespace Ink.Worlds.Biomes;

public readonly record struct BiomeAdditionsSound(Identifier Sound, double TickChance)
{
    public readonly Identifier Sound = Sound;
    public readonly double TickChance = TickChance;
}

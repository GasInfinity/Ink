using Ink.Registries;

namespace Ink.World.Biomes;

public readonly record struct BiomeAdditionsSound
{
    public required Identifier Sound { get; init; }
    public required double TickChance { get; init; }
}

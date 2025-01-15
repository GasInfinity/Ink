using Ink.Registries;

namespace Ink.World.Biomes;

public readonly record struct BiomeMusic
{
    public required Identifier Sound { get; init; }
    public required int MinDelay { get; init; }
    public required int MaxDelay { get; init; }
    public required bool ReplaceCurrentMusic { get; init; }

    public BiomeMusic()
    {
    }
}

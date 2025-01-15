using Ink.Registries;

namespace Ink.World.Biomes;

public readonly record struct BiomeMoodSound
{
    public required Identifier Sound { get; init; }
    public required int TickDelay { get; init; }
    public required int BlockSearchExtent { get; init; }
    public required double Offset { get; init; }

    public BiomeMoodSound()
    {
    }
}

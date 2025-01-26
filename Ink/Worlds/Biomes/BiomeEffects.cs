using System.Runtime.InteropServices;
using Ink.Registries;

namespace Ink.Worlds.Biomes;

[StructLayout(LayoutKind.Auto)]
public readonly record struct BiomeEffects
{
    public required int FogColor { get; init; }
    public required int SkyColor { get; init; }
    public required int WaterColor { get; init; }
    public required int WaterFogColor { get; init; }
    public int? FoliageColor { get; init; }
    public int? GrassColor { get; init; }
    public BiomeGrassColorModifier GrassColorModifier { get; init; }
    public Identifier? AmbientSound { get; init; }
    public BiomeMoodSound? MoodSound { get; init; }
    public BiomeAdditionsSound? AdditionsSound { get; init; }
    public BiomeMusic? Music { get; init; }

    public BiomeEffects()
    {
    }
}

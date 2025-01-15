using System.Runtime.InteropServices;

namespace Ink.World.Biomes;

[StructLayout(LayoutKind.Auto)]
public sealed record BiomeType
{
    public required bool HasPrecipitation { get; init; }
    public required float Temperature { get; init; }
    public BiomeTemperatureModifier TemperatureModifier { get; init; } = BiomeTemperatureModifier.None;
    public required float Downfall { get; init; }
    public required BiomeEffects Effects { get; init; }

    public BiomeType()
    {
    }
}

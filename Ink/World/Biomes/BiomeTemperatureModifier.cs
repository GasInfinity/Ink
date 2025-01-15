using System.ComponentModel.DataAnnotations;
using NetEscapades.EnumGenerators;

namespace Ink.World.Biomes;

[EnumExtensions]
public enum BiomeTemperatureModifier : byte
{
    [Display(Name = "none")] None,
    [Display(Name = "frozen")] Frozen
}

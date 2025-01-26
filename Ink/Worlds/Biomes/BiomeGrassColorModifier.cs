using System.ComponentModel.DataAnnotations;
using NetEscapades.EnumGenerators;

namespace Ink.Worlds.Biomes;

[EnumExtensions]
public enum BiomeGrassColorModifier : byte
{
    [Display(Name = "none")] None,
    [Display(Name = "dark_forest")] DarkForest,
    [Display(Name = "swamp")] Swamp
}

using System.ComponentModel.DataAnnotations;
using NetEscapades.EnumGenerators;

namespace Ink.Blocks;

[EnumExtensions]
public enum WallBlockConnection
{
    [Display(Name = "none")] None,
    [Display(Name = "low")] Low,
    [Display(Name = "tall")] Tall
}

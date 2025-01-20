using System.ComponentModel.DataAnnotations;
using NetEscapades.EnumGenerators;

namespace Ink.Blocks;

[EnumExtensions]
public enum TallBlockPart
{
    [Display(Name = "upper")] Upper,
    [Display(Name = "lower")] Lower
}

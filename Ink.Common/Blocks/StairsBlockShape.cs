using System.ComponentModel.DataAnnotations;
using NetEscapades.EnumGenerators;

namespace Ink.Blocks;

[EnumExtensions]
public enum StairsBlockShape
{
    [Display(Name = "straight")] Straight,
    [Display(Name = "inner_left")] InnerLeft,
    [Display(Name = "inner_right")] InnerRight,
    [Display(Name = "outer_left")] OuterLeft,
    [Display(Name = "outer_right")] OuterRight,
}

using System.ComponentModel.DataAnnotations;
using NetEscapades.EnumGenerators;

namespace Ink.Blocks;

[EnumExtensions]
public enum SlabBlockType
{
    [Display(Name = "top")] Top,
    [Display(Name = "bottom")] Bottom,
    [Display(Name = "double")] Double
}

using System.ComponentModel.DataAnnotations;
using NetEscapades.EnumGenerators;

namespace Ink.Blocks.State;

[EnumExtensions]
public enum BellBlockAttachment
{
    [Display(Name = "floor")] Floor,
    [Display(Name = "ceiling")] Ceiling,
    [Display(Name = "single_wall")] SingleWall,
    [Display(Name = "double_wall")] DoubleWall,
}

using System.ComponentModel.DataAnnotations;
using NetEscapades.EnumGenerators;

namespace Ink.Blocks.State;

[EnumExtensions]
public enum ButtonBlockAttachment
{
    [Display(Name = "wall")] Wall,
    [Display(Name = "floor")] Floor,
    [Display(Name = "ceiling")] Ceiling
}

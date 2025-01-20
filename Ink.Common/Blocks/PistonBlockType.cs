using System.ComponentModel.DataAnnotations;
using NetEscapades.EnumGenerators;

namespace Ink.Blocks.State;

[EnumExtensions]
public enum PistonBlockType
{
    [Display(Name = "normal")] Normal,
    [Display(Name = "sticky")] Sticky
}

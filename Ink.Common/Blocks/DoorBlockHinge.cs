using System.ComponentModel.DataAnnotations;
using NetEscapades.EnumGenerators;

namespace Ink.Blocks;

[EnumExtensions]
public enum DoorBlockHinge
{
    [Display(Name = "left")] Left,
    [Display(Name = "right")] Right
}

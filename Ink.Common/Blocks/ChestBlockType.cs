using System.ComponentModel.DataAnnotations;
using NetEscapades.EnumGenerators;

namespace Ink.Blocks;

[EnumExtensions]
public enum ChestBlockType 
{
    [Display(Name = "single")] Single,
    [Display(Name = "left")] Left,
    [Display(Name = "right")] Right,
}

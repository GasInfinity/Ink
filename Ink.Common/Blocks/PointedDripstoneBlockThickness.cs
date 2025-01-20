using System.ComponentModel.DataAnnotations;
using NetEscapades.EnumGenerators;

namespace Ink.Blocks.State;

[EnumExtensions]
public enum PointerDripstoneBlockThickness 
{
    [Display(Name = "tip_merge")] TipMerge,
    [Display(Name = "tip")] Tip,
    [Display(Name = "frustum")] Frustum,
    [Display(Name = "middle")] Middle,
    [Display(Name = "base")] Base,
}

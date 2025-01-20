using System.ComponentModel.DataAnnotations;
using NetEscapades.EnumGenerators;

namespace Ink.Blocks;

[EnumExtensions]
public enum BedBlockPart
{
    [Display(Name = "head")] Head,
    [Display(Name = "foot")] Foot 
}

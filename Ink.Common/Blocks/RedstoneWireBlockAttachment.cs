using System.ComponentModel.DataAnnotations;
using NetEscapades.EnumGenerators;

namespace Ink.Blocks.State;

[EnumExtensions]
public enum RedstoneWireBlockAttachment
{
    [Display(Name = "none")] None,
    [Display(Name = "up")] Up,
    [Display(Name = "side")] Side 
}

using System.ComponentModel.DataAnnotations;
using NetEscapades.EnumGenerators;

namespace Ink.Blocks;

[EnumExtensions]
public enum BambooBlockLeaves 
{
    [Display(Name = "none")] None,
    [Display(Name = "small")] Small, 
    [Display(Name = "large")] Large, 
}

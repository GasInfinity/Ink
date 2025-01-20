using System.ComponentModel.DataAnnotations;
using NetEscapades.EnumGenerators;

namespace Ink.Blocks;

[EnumExtensions]
public enum VaultBlockState 
{
    [Display(Name = "inactive")] Inactive,
    [Display(Name = "active")] Active,
    [Display(Name = "unlocking")] Unlocking,
    [Display(Name = "ejecting")] Ejecting,
}

using System.ComponentModel.DataAnnotations;
using NetEscapades.EnumGenerators;

namespace Ink.Blocks;

[EnumExtensions]
public enum DripleafBlockTilt 
{
    [Display(Name = "none")] None,
    [Display(Name = "unstable")] Unstable, 
    [Display(Name = "partial")] Partial, 
    [Display(Name = "full")] Full, 
}

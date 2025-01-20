using System.ComponentModel.DataAnnotations;
using NetEscapades.EnumGenerators;

namespace Ink.Blocks;

[EnumExtensions]
public enum ComparatorBlockMode 
{
    [Display(Name = "compare")] Compare,
    [Display(Name = "subtract")] Subtract,
}

using System.ComponentModel.DataAnnotations;
using NetEscapades.EnumGenerators;

namespace Ink.Blocks;

[EnumExtensions]
public enum StructureBlockMode 
{
    [Display(Name = "save")] Save,
    [Display(Name = "load")] Load,
    [Display(Name = "corner")] Corner,
    [Display(Name = "data")] Data,

}

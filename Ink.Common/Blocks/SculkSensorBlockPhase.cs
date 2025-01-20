using System.ComponentModel.DataAnnotations;
using NetEscapades.EnumGenerators;

namespace Ink.Blocks.State;

[EnumExtensions]
public enum SculkSensorBlockPhase
{
    [Display(Name = "inactive")] Inactive,
    [Display(Name = "active")] Active,
    [Display(Name = "cooldown")] Cooldown 
}

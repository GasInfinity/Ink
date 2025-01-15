using Ink.Util.Converter.Json;
using NetEscapades.EnumGenerators;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace Ink.Entities.Damage;

[EnumExtensions]
[JsonConverter(typeof(LowerSnakeEnumJsonConverter<DamageEffect>))]
public enum DamageEffect
{
    [Display(Name = "hurt")] Hurt,
    [Display(Name = "thorns")] Thorns,
    [Display(Name = "drowning")] Drowning,
    [Display(Name = "burning")] Burning,
    [Display(Name = "poking")] Poking,
    [Display(Name = "freezing")] Freezing
}

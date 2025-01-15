using Ink.Util.Converter.Json;
using NetEscapades.EnumGenerators;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace Ink.Entities.Damage;

[EnumExtensions]
[JsonConverter(typeof(LowerSnakeEnumJsonConverter<DamageScaling>))]
public enum DamageScaling
{
    [Display(Name = "never")] Never,
    [Display(Name = "always")] Always,
    [Display(Name = "when_caused_by_living_non_player")] WhenCausedByLivingNonPlayer
}

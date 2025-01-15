using Ink.Util.Converter.Json;
using NetEscapades.EnumGenerators;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace Ink.Entities.Damage;

[EnumExtensions]
[JsonConverter(typeof(LowerSnakeEnumJsonConverter<DeathMessageType>))]
public enum DeathMessageType
{
    [Display(Name = "default")] Default,
    [Display(Name = "fall_variants")] FallVariants,
    [Display(Name = "intentional_game_design")] IntentionalGameDesign
}

using System.ComponentModel.DataAnnotations;
using NetEscapades.EnumGenerators;

namespace Ink.Blocks;

[EnumExtensions]
public enum TrialSpawnerBlockState 
{
    [Display(Name = "inactive")] Inactive,
    [Display(Name = "waiting_for_players")] WaitingForPlayers, 
    [Display(Name = "active")] Active, 
    [Display(Name = "waiting_for_reward_ejection")] WaitingForRewardEjection, 
    [Display(Name = "ejecting_reward")] EjectingReward, 
    [Display(Name = "cooldown")] Cooldown, 
}

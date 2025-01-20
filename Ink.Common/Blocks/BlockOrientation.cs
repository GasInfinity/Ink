using System.ComponentModel.DataAnnotations;
using NetEscapades.EnumGenerators;

namespace Ink.Blocks.State;

[EnumExtensions]
public enum BlockOrientation 
{
    [Display(Name = "down_east")] DownEast,
    [Display(Name = "down_west")] DownWest,
    [Display(Name = "down_north")] DownNorth,
    [Display(Name = "down_south")] DownSouth,
    [Display(Name = "up_east")] UpEast,
    [Display(Name = "up_west")] UpWest,
    [Display(Name = "up_north")] UpNorth,
    [Display(Name = "up_south")] UpSouth,
    [Display(Name = "west_up")] WestUp,
    [Display(Name = "east_up")] EastUp,
    [Display(Name = "north_up")] NorthUp,
    [Display(Name = "south_up")] SouthUp,
}

using System.ComponentModel.DataAnnotations;
using NetEscapades.EnumGenerators;

namespace Ink.Blocks.State;

[EnumExtensions]
public enum RailBlockShape 
{
    [Display(Name = "north_south")] NorthSouth,
    [Display(Name = "east_west")] EastWest,
    [Display(Name = "ascending_east")] AscendingEast,
    [Display(Name = "ascending_west")] AscendingWest,
    [Display(Name = "ascending_north")] AscendingNorth,
    [Display(Name = "ascending_south")] AscendingSouth,
    [Display(Name = "south_east")] SouthEast,
    [Display(Name = "south_west")] SouthWest,
    [Display(Name = "north_west")] NorthWest,
    [Display(Name = "north_east")] NorthEast,
}

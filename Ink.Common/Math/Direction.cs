using System.ComponentModel.DataAnnotations;
using NetEscapades.EnumGenerators;

namespace Ink.Math;

public sealed record Direction
{
    public static readonly Direction Up = new(Directions.Up, Axes.Y);
    public static readonly Direction Down = new(Directions.Down, Axes.Y);
    public static readonly Direction North = new(Directions.North, Axes.Z);
    public static readonly Direction South = new(Directions.South, Axes.Z);
    public static readonly Direction West = new(Directions.West, Axes.X);
    public static readonly Direction East = new(Directions.East, Axes.X);

    private static readonly Direction[] Values = [North, East, South, West, Up, Down];
    private static readonly Direction[] Opposites = [South, West, North, East, Down, Up];
    private static readonly Direction[] AxisValues = [East, Up, North];

    public readonly Directions Value;
    public readonly Axes Axis;

    public Direction Opposite
        => Opposites[(int)Value];

    private Direction(Directions value, Axes axis)
        => (Value, Axis) = (value, axis);

    public static Direction From(Directions value)
        => Values[(int)value];

    public static Direction From(Axes value)
        => AxisValues[(int)value];

    public static Direction FromRotation(float yaw)
    {
        int wrapped360 = (int)yaw % 360;

        if (wrapped360 < 0) // Return the opposite
        {
            return -wrapped360 switch
            {
                > 315 => South,
                > 225 => West,
                > 135 => North,
                > 45 => East,
                _ => South
            };
        }
        else
        {
            return wrapped360 switch
            {
                > 315 => South,
                > 225 => East,
                > 135 => North,
                > 45 => West,
                _ => South
            };
        }
    }

    public static Direction FromRotations(float yaw, float pitch)
    {
        int pitchInt = (int)float.Abs(pitch) % 360;

        if (pitchInt > 75)
            return Up;

        if (pitchInt < -75)
            return Down;

        return FromRotation(yaw);
    }

    [EnumExtensions]
    public enum Directions : byte
    {
        [Display(Name = "north")] North,
        [Display(Name = "east")] East,
        [Display(Name = "south")] South,
        [Display(Name = "west")] West,
        [Display(Name = "up")] Up,
        [Display(Name = "down")] Down
    }

    [EnumExtensions]
    public enum Axes : byte
    {
        [Display(Name = "x")] X,
        [Display(Name = "y")] Y,
        [Display(Name = "z")] Z
    }
}

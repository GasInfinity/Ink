namespace Ink.Math;

public sealed record Direction
{
    public static readonly Direction Up = new(Directions.Up, HorizontalDirections.North, HopperDirections.Down, Axes.Y, HorizontalAxes.X);
    public static readonly Direction Down = new(Directions.Down, HorizontalDirections.North, HopperDirections.Down, Axes.Y, HorizontalAxes.X);
    public static readonly Direction North = new(Directions.North, HorizontalDirections.North, HopperDirections.North, Axes.Z, HorizontalAxes.Z);
    public static readonly Direction South = new(Directions.South, HorizontalDirections.South, HopperDirections.South, Axes.Z, HorizontalAxes.Z);
    public static readonly Direction West = new(Directions.West, HorizontalDirections.West, HopperDirections.West, Axes.X, HorizontalAxes.X);
    public static readonly Direction East = new(Directions.East, HorizontalDirections.East, HopperDirections.East, Axes.X, HorizontalAxes.X);

    private static readonly Direction[] Values = [North, East, South, West, Up, Down];
    private static readonly Direction[] Opposites = [South, West, North, East, Down, Up];
    private static readonly Direction[] HopperValues = [Down, North, South, West, East];
    private static readonly Direction[] HorizontalValues = [North, South, West, East];
    private static readonly Direction[] AxisValues = [East, Up, North];
    private static readonly Direction[] HorizontalAxisValues = [East, North];

    public readonly Directions Value;
    public readonly HorizontalDirections Horizontal;
    public readonly HopperDirections Hopper;
    public readonly Axes Axis;
    public readonly HorizontalAxes HorizontalAxis;

    public Direction Opposite
        => Opposites[(int)Value];

    private Direction(Directions value, HorizontalDirections horizontal, HopperDirections hopper, Axes axis, HorizontalAxes hAxis)
        => (Value, Horizontal, Hopper, Axis, HorizontalAxis) = (value, horizontal, hopper, axis, hAxis);

    public static Direction From(Directions value)
        => Values[(int)value];

    public static Direction From(HopperDirections value)
        => HopperValues[(int)value];

    public static Direction From(HorizontalDirections value)
        => HorizontalValues[(int)value];

    public static Direction From(Axes value)
        => AxisValues[(int)value];

    public static Direction From(HorizontalAxes value)
        => HorizontalAxisValues[(int)value];

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

    public enum Directions : byte
    {
        North,
        East,
        South,
        West,
        Up,
        Down
    }

    public enum HopperDirections : byte
    {
        Down,
        North,
        South,
        West,
        East
    }

    public enum HorizontalDirections : byte
    {
        North,
        South,
        West,
        East
    }

    public enum Axes : byte
    {
        X,
        Y,
        Z
    }

    public enum HorizontalAxes : byte
    {
        X,
        Z
    }
}

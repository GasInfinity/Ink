namespace Ink.Math;

public enum BlockFace : byte
{
    Bottom,
    Top,
    North,
    South,
    West,
    East,
}

public static class BlockFaceExtensions
{
    private static readonly Direction[] Directions = [Direction.Down, Direction.Up, Direction.North, Direction.South, Direction.West, Direction.East];

    public static Direction ToDirection(this BlockFace face)
        => Directions[(int)face];
}
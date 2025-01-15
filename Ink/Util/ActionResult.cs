namespace Ink.Util;

public enum ActionResult
{
    Consume,
    ConsumePartially,
    Fail,
    Pass,
    Success
}

public static class ActionResultExtensions
{
    private static ReadOnlySpan<bool> Performs => new bool[] { true, true, false, false, true };
    private static ReadOnlySpan<bool> Swings => new bool[] { false, false, false, false, true };

    public static bool PerformsAction(this ActionResult action)
        => Performs[(int)action];

    public static bool SwingsHand(this ActionResult action)
        => Swings[(int)action];
}
namespace Ink.Util;

public readonly record struct ActionResult<T>(ActionResult Result, T? Value)
{
    public static ActionResult<T> Pass
        => new(ActionResult.Pass, default);

    public readonly ActionResult Result = Result;
    public readonly T? Value = Value;

    public static ActionResult<T> Success(T value)
        => new(ActionResult.Success, value);
}

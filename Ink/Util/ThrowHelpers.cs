using System.Diagnostics.CodeAnalysis;

namespace Ink.Util;

public static class ThrowHelpers
{
    [DoesNotReturn]
    public static void ThrowTickingWhileTicked()
        => throw new InvalidOperationException($"We are being ticked while we are ticking, are you sure about doing this?!");
}

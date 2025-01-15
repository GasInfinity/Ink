using System.Runtime.CompilerServices;

namespace Ink.Blocks.State;

public enum PropertyKind
{
    Boolean,
    Integer
}

public static class PropertyKindExtensions
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsCompatible<T>(this PropertyKind type)
        where T : unmanaged
        => type switch
        {
            PropertyKind.Integer => (typeof(T) == typeof(byte))
                                 || (typeof(T) == typeof(short))
                                 || (typeof(T) == typeof(int))
                                 || (typeof(T) == typeof(long))
                                 || (typeof(T) == typeof(sbyte))
                                 || (typeof(T) == typeof(ushort))
                                 || (typeof(T) == typeof(uint))
                                 || (typeof(T) == typeof(ulong))
                                 || (typeof(T) == typeof(nint))
                                 || (typeof(T) == typeof(nuint))
                                 || (typeof(T).IsAssignableTo(typeof(Enum))),
            PropertyKind.Boolean => typeof(T) == typeof(bool),
            _ => false
        };
}

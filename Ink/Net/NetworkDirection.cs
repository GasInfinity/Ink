namespace Ink.Net;

public enum NetworkDirection : byte
{
    /// <summary>
    /// S -> C
    /// </summary>
    Clientbound,
    /// <summary>
    /// C -> S
    /// </summary>
    Serverbound
}

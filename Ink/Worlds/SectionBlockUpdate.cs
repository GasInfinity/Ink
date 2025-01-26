namespace Ink.Worlds;

public readonly record struct SectionBlockUpdate(int RelX, int RelY, int RelZ, int BlockState)
{
    public readonly long Raw = ((long)BlockState << 12) | (((long)RelX & 0xF) << 8) | (((long)RelZ & 0xF) << 4) | ((long)RelY & 0xF);

    public int RelX
        => (int)((Raw >>> 8) & 0xF);

    public int RelY
        => (int)(Raw & 0xF);
    
    public int RelZ
        => (int)((Raw >>> 4) & 0xF);

    public int BlockState
        => (int)(Raw >>> 12);
}

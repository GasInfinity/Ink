namespace Ink.Blocks.State;

public readonly record struct Property(int Value)
{
    public readonly byte Raw = (byte)(Value);
    
    public int Value
        => Raw;
}

namespace Ink.Chunks.Pallete;

public interface IPalettedValue<T>
    where T : IPalettedValue<T>
{
    static abstract byte MinIndirectBits { get; }
    static abstract byte MaxIndirectBits { get; }
    static abstract byte DirectBits { get; }

    int PaletteId { get; }

    static abstract T FromPaletteId(int id);
}

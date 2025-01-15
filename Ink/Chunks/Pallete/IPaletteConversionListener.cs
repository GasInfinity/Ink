namespace Ink.Chunks.Pallete;

public interface IPaletteConversionListener<T>
    where T : IPalettedValue<T>
{
    void OnIndirectToDirectPaletteConversion(ReadOnlySpan<int> indirectPalette, Empty signature);

    public readonly struct Empty
    {
    }
}

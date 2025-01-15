using System.Buffers;

namespace Ink.Chunks.Pallete;

public interface IPalette<T>
    where T : IPalettedValue<T>
{
    public const int InvalidPaletteIndex = -1;

    SectionPaletteType Type { get; }

    int GetOrAddIndex<TListener>(T value, ref TListener listener)
        where TListener : IPaletteConversionListener<T>;

    void Write(IBufferWriter<byte> writer);
}

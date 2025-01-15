using Rena.Native.Buffers.Extensions;
using Rena.Native.Extensions;
using System.Buffers;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Ink.Util.Extensions;
using Ink.Util;

namespace Ink.Chunks.Pallete;

/// <summary>
/// TODO: allow custom ArrayPool? See the FIXME in <see cref="PooledCompactedData"/>
/// </summary>
/// <typeparam name="T">The type to store</typeparam>
public struct PooledSectionPalette<T>(byte initialBits) : IPalette<T>, IDisposable
    where T : IPalettedValue<T>
{
    /// <summary>
    /// The palette, when its null or 0 this is a direct palette
    /// </summary>
    private byte[]? rawPalette = CreateArrayFromBits(initialBits);
    private int count = 0;

    private readonly Span<int> Palette
        => MemoryMarshal.CreateSpan(ref Unsafe.As<byte, int>(ref MemoryMarshal.GetArrayDataReference(this.rawPalette!)), (int)((uint)this.rawPalette!.Length / sizeof(int)));

    /// <summary>
    /// Should only be used when Type == Indirect
    /// </summary>
    private readonly byte IndirectBitsUsed
        => (byte)Utilities.BitSize(this.rawPalette!.Length / sizeof(int));

    public readonly SectionPaletteType Type
        => this.rawPalette == null ? SectionPaletteType.Direct : this.rawPalette.Length switch
        {
            0 => SectionPaletteType.Direct,
            (sizeof(int)) => SectionPaletteType.SingleValued,
            _ => SectionPaletteType.Indirect
        };

    public readonly byte BitsUsed
        => Type switch
        {
            SectionPaletteType.Direct => T.DirectBits,
            SectionPaletteType.SingleValued => 0,
            SectionPaletteType.Indirect => IndirectBitsUsed,
            _ => T.DirectBits // Should never happen
        };

    public readonly bool CurrentlyFull
        => this.rawPalette != null && this.rawPalette.Length <= count;

    public int GetOrAddIndex<TListener>(T value, ref TListener listener)
        where TListener : IPaletteConversionListener<T>
    {
        if (Type == SectionPaletteType.Direct)
            return value.PaletteId;

        Debug.Assert(this.rawPalette != null);
        Span<int> palette = this.Palette;
        int paletteId = value.PaletteId;
        int paletteIndex = palette.IndexOf(paletteId);
        int safePaletteIndex = paletteIndex - (paletteIndex != -1 && this.count == 0).AsByte(); // Avoid getting default(int) if we don't have values, we must add it always

        if (safePaletteIndex == -1)
            return AddNewIndex(paletteId, ref listener);

        return safePaletteIndex;
    }

    public readonly T GetValue(int paletteIndex)
    {
        if (Type == SectionPaletteType.Direct)
            return T.FromPaletteId(paletteIndex);

        Span<int> palette = this.Palette;
        Debug.Assert(paletteIndex < palette!.Length);
        return T.FromPaletteId(palette[paletteIndex]);
    }

    public void Clear(bool useDirect)
    {
        DisposeIfPooled();
        this.rawPalette = useDirect ? [] : new byte[sizeof(int)];
        this.count = 0;
    }

    public readonly void Write(IBufferWriter<byte> writer)
    {
        switch (Type)
        {
            case SectionPaletteType.SingleValued:
                {
                    Span<int> palette = this.Palette;
                    Debug.Assert(palette.Length == 1);

                    writer.WriteRaw((byte)0);
                    writer.WriteVarInteger(this.Palette.GetUnsafe(0));
                    break;
                }
            case SectionPaletteType.Indirect:
                {
                    Span<int> palette = this.Palette;

                    writer.WriteRaw(IndirectBitsUsed);
                    writer.WriteVarInteger(this.count);

                    for (int i = 0; i < this.count; ++i)
                        writer.WriteVarInteger(palette.GetUnsafe(i));
                    break;
                }
            case SectionPaletteType.Direct:
                {
                    writer.WriteRaw(T.DirectBits);
                    break;
                }
        }
    }

    public void Dispose()
        => Clear(true);

    private int AddNewIndex<TListener>(int paletteValue, ref TListener listener)
        where TListener : IPaletteConversionListener<T>
    {
        Debug.Assert(this.rawPalette != null);
        Span<int> palette = this.Palette;
        int newIndex = this.count++;

        if(newIndex >= palette.Length)
        {
            // Grow
            byte bitsUsed = (byte)(BitsUsed + 1);

            if(bitsUsed > T.MaxIndirectBits)
            {
                listener.OnIndirectToDirectPaletteConversion(palette, default);
                Clear(true);
                return paletteValue;
            }

            byte[] newPalette = CreateArrayFromBits(bitsUsed);
            Array.Copy(this.rawPalette, newPalette, this.rawPalette.Length);

            DisposeIfPooled();
            this.rawPalette = newPalette;
            palette = this.Palette;
        }

        palette[newIndex] = paletteValue;
        return newIndex;
    }

    private void DisposeIfPooled()
    {
        if (this.rawPalette?.Length > sizeof(int))
            ArrayPool<byte>.Shared.Return(this.rawPalette, true);
        this.rawPalette = [];
    }

    public static byte[] CreateArrayFromBits(byte bitsPerIndex)
    {
        if (bitsPerIndex == 0)
            return new byte[sizeof(int)];

        if (bitsPerIndex < T.MinIndirectBits)
            bitsPerIndex = T.MinIndirectBits;

        if (bitsPerIndex <= T.MaxIndirectBits)
        {
            byte[] rented = ArrayPool<byte>.Shared.Rent((1 << bitsPerIndex) * sizeof(int));
            Array.Clear(rented);
            return rented;
        }

        return [];
    }
}

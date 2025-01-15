using Rena.Native.Buffers.Extensions;
using Rena.Native.Extensions;
using System.Buffers;
using System.Diagnostics;
using Ink.Chunks.Pallete;
using Ink.Util;

namespace Ink.Chunks;

public struct Section : IDisposable, IPaletteConversionListener<StateStorage>, IPaletteConversionListener<BiomeStorage>
{
    public const int Size = 16;
    public const int Surface = Size * Size;
    public const int Volume = Surface * Size;
    public static readonly int ShiftAmount = (int)uint.Log2(Size);

    public const int BlockCount = Size * Size * Size;

    public const int BiomeSize = 4;
    public const int BiomeSurface = BiomeSize * BiomeSize;
    public const int BiomeCount = BiomeSize * BiomeSize * BiomeSize;

    private PooledSectionPalette<StateStorage> blockPalette;
    private PooledCompactedData blockData;

    private PooledSectionPalette<BiomeStorage> biomePalette;
    private PooledCompactedData biomeData;

    private short nonEmptyCount;

    public readonly short NonEmptyCount
        => nonEmptyCount;

    public Section()
    {
        FillBlock(default);
        FillBiome(default);
    }

    public StateStorage SetBlock(int x, int y, int z, StateStorage state)
    {
        if ((uint)x >= Size || (uint)y >= Size || (uint)z >= Size)
            return default; // We can't change something out of bounds

        int paletteIndex = this.blockPalette.GetOrAddIndex(state, ref this);

        if (this.blockPalette.BitsUsed == 0)
            return default; // Do nothing, we have only 1 state, and that state is what we passed to the method!

        if (this.blockPalette.BitsUsed > this.blockData.BitsPerEntry)
            this.blockData = this.blockData.Grow(BlockCount, this.blockPalette.BitsUsed);

        int index = BlockIndex(x, y, z);
        ref long dataIndex = ref this.blockData.GetLocationUnsafely(index, out int dataBitIndex);
        int lastPaletteIndex = this.blockData.GetUnsafely(ref dataIndex, dataBitIndex);
        this.blockData.SetUnsafely(ref dataIndex, dataBitIndex, paletteIndex);

        StateStorage lastState = this.blockPalette.GetValue(lastPaletteIndex);
        this.nonEmptyCount += (short)((!state.IsAir).AsByte() - (!lastState.IsAir).AsByte());
        return lastState;
    }

    /// <summary>
    /// Fills this section with a given block(state).
    /// </summary>
    /// <param name="state">The block(state) to fill this section with</param>
    public void FillBlock(StateStorage state)
    {
        this.blockPalette.Clear(false);
        _ = this.blockPalette.GetOrAddIndex(state, ref this);
        this.blockData.Dispose();
        this.blockData = default;
        this.nonEmptyCount = (short)((!state.IsAir).AsByte() * BlockCount); // Filled or not filled!
    }

    public readonly StateStorage GetBlock(int x, int y, int z)
    {
        if ((uint)x >= Size || (uint)y >= Size || (uint)z >= Size)
            return default;

        if (this.blockPalette.Type == SectionPaletteType.SingleValued)
            return this.blockPalette.GetValue(0);

        int index = BlockIndex(x, y, z);
        return this.blockPalette.GetValue(this.blockData[index]);
    }

    public bool SetBiome(int x, int y, int z, BiomeStorage biome)
    {
        if ((uint)x >= BiomeSize || (uint)y >= BiomeSize || (uint)z >= BiomeSize)
            return false;

        int paletteIndex = this.biomePalette.GetOrAddIndex(biome, ref this);

        if (this.biomePalette.BitsUsed == 0)
            return false; // Do nothing, single valued!

        if (this.biomePalette.BitsUsed > this.biomeData.BitsPerEntry)
            this.biomeData = this.biomeData.Grow(BiomeCount, this.biomePalette.BitsUsed);

        int index = BiomeIndex(x, y, z);
        ref long dataIndex = ref this.biomeData.GetLocationUnsafely(index, out int dataBitIndex);
        int lastPaletteIndex = this.biomeData.GetUnsafely(ref dataIndex, dataBitIndex);
        this.biomeData.SetUnsafely(ref dataIndex, dataBitIndex, paletteIndex);

        BiomeStorage lastBiome = this.biomePalette.GetValue(lastPaletteIndex);
        return lastBiome != biome;
    }

    /// <summary>
    /// Fills this section with a given biome.
    /// </summary>
    /// <param name="biome">The biome to fill this section with</param>
    public void FillBiome(BiomeStorage biome)
    {
        this.biomePalette.Clear(false);
        _ = this.biomePalette.GetOrAddIndex(biome, ref this);
        this.biomeData.Dispose();
        this.biomeData = default;
    }

    public readonly BiomeStorage GetBiome(int x, int y, int z)
    {
        if ((uint)x >= BiomeSize || (uint)y >= BiomeSize || (uint)z >= BiomeSize)
            return default;

        if (this.biomePalette.Type == SectionPaletteType.SingleValued)
            return this.biomePalette.GetValue(0);

        int index = BiomeIndex(x, y, z);
        return this.biomePalette.GetValue(this.biomeData[index]);
    }

    public void OnIndirectToDirectPaletteConversion(ReadOnlySpan<int> indirectPalette, IPaletteConversionListener<StateStorage>.Empty signature)
    {
        Debug.Assert(indirectPalette.Length != 1, "Must never happen, just to be sure. this.blockData is in a default state...");
        PooledCompactedData newData = new(BlockCount, StateStorage.DirectBits);

        for (int i = 0; i < BlockCount; ++i)
            newData[i] = indirectPalette.GetUnsafe(this.blockData[i]);

        this.blockData.Dispose();
        this.blockData = newData;
    }

    public void OnIndirectToDirectPaletteConversion(ReadOnlySpan<int> indirectPalette, IPaletteConversionListener<BiomeStorage>.Empty signature)
    {
        Debug.Assert(indirectPalette.Length != 1, "Must never happen, just to be sure. this.biomeData is in a default state...");
        PooledCompactedData newData = new(BiomeCount, BiomeStorage.DirectBits);

        for (int i = 0; i < BlockCount; ++i)
            newData[i] = indirectPalette.GetUnsafe(this.biomeData[i]);

        this.biomeData.Dispose();
        this.biomeData = newData;
    }

    public readonly void Write(IBufferWriter<byte> writer)
    {
        writer.WriteInt16(this.nonEmptyCount, false);
        this.blockPalette.Write(writer);
        this.blockData.Write(writer);
        this.biomePalette.Write(writer);
        this.biomeData.Write(writer);
    }

    public void Dispose()
    {
        this.blockPalette.Dispose();
        this.blockData.Dispose();
        this.biomePalette.Dispose();
        this.biomeData.Dispose();
        this = default;
    }

    public static int BlockIndex(int sectionX, int sectionY, int sectionZ)
        => sectionX + (sectionZ * Size) + (sectionY * Surface);

    public static int BiomeIndex(int biomeSectionX, int biomeSectionY, int biomeSectionZ)
        => biomeSectionX + (biomeSectionZ * BiomeSize) + (biomeSectionY * BiomeSurface);
}

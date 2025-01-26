using Ink.Util;
using Ink.Util.Extensions;
using Rena.Native.Buffers;
using System.Buffers;
using System.Diagnostics;

namespace Ink.Chunks;

public struct Chunk : IDisposable
{
    public const int HorizontalSize = Section.Size;
    public const int HorizontalSurface = Section.Surface;
    public static readonly int ShiftAmount = (int)uint.Log2(HorizontalSize);

    private Section[] sections;
    private LightSection[] lightSections;

    public readonly bool IsCreated
        => this.sections != null;

    public readonly int Height
        => this.sections.Length * Section.Size;

    public readonly object SyncRoot
        => this.sections; // Well, I don't want to waste 8KiB of objects only for locking

    public Chunk(int minY, int height)
    {
        Debug.Assert((minY % Section.Size) == 0 & (height % Section.Size) == 0 & int.IsPositive(height), "Height must be a valid value");
        this.sections = new Section[(height - minY) / HorizontalSize]; // 64 - 16 / 16 = 3
        this.lightSections = new LightSection[this.sections.Length];

        for (int i = 0; i < this.sections.Length; i++)
            this.sections[i] = new Section();
    }

    public readonly StateStorage SetBlockState(int minY, int x, int y, int z, StateStorage state)
    {
        Debug.Assert(this.sections != null);
        (int sectionIndex, int sectionYCoordinate) = int.DivRem(y - minY, Section.Size);

        if ((uint)x >= HorizontalSize 
        || (uint)sectionIndex >= this.sections.Length
        || (uint)z >= HorizontalSize)
        {
            return default;
        }

        ref Section section = ref this.sections[sectionIndex];
        return section.SetBlock(x, sectionYCoordinate, z, state);
    }

    public readonly StateStorage GetBlockState(int minY, int x, int y, int z)
    {
        Debug.Assert(this.sections != null);
        (int sectionIndex, int sectionYCoordinate) = int.DivRem(y - minY, Section.Size);

        if ((uint)x >= HorizontalSize 
        || (uint)sectionIndex >= this.sections.Length 
        || (uint)z >= HorizontalSize)
        {
            return default;
        }

        ref Section section = ref this.sections[sectionIndex];
        return section.GetBlock(x, sectionYCoordinate, z);
    }

    public bool SetBiome(int minY, int x, int y, int z, BiomeStorage biome)
    {
        Debug.Assert(this.sections != null);
        (int sectionIndex, int sectionYCoordinate) = int.DivRem(y - minY, Section.Size);

        if ((uint)x >= HorizontalSize
        || (uint)sectionIndex >= this.sections.Length
        || (uint)z >= HorizontalSize)
        {
            return false;
        }

        ref Section section = ref this.sections[sectionIndex];
        return section.SetBiome(x / Section.BiomeSize, sectionYCoordinate / Section.BiomeSize, z / Section.BiomeSize, biome);
    }

    public BiomeStorage GetBiome(int minY, int x, int y, int z)
    {
        Debug.Assert(this.sections != null);
        (int sectionIndex, int sectionYCoordinate) = int.DivRem(y - minY, Section.Size);

        if ((uint)x >= HorizontalSize
        || (uint)sectionIndex >= this.sections.Length
        || (uint)z >= HorizontalSize)
        {
            return default;
        }

        ref Section section = ref this.sections[sectionIndex];
        return section.GetBiome(x / Section.BiomeSize, sectionYCoordinate / Section.BiomeSize, z / Section.BiomeSize);
    }

    public void FillBlock(StateStorage state)
    {
        Debug.Assert(this.sections != null);

        for (int i = 0; i < this.sections.Length; i++)
        {
            ref Section section = ref this.sections[i];
            section.FillBlock(state);
        }
    }

    public void FillBiome(BiomeStorage biome)
    {
        Debug.Assert(this.sections != null);

        for (int i = 0; i < this.sections.Length; ++i)
        {
            ref Section section = ref this.sections[i];
            section.FillBiome(biome);
        }
    }

    public readonly void Write(IBufferWriter<byte> writer)
    {
        using PooledArrayBufferWriter<byte> sectionWriter = Utilities.SharedBufferWriters.Get();

        for (int i = 0; i < this.sections.Length; ++i)
        {
            ref Section section = ref this.sections[i];
            section.Write(sectionWriter);
        }

        writer.WriteVarInteger(sectionWriter.WrittenCount);
        writer.Write(sectionWriter.WrittenSpan);

        writer.WriteVarInteger(0); // TODO!: BlockEntities
        Utilities.SharedBufferWriters.Return(sectionWriter);
    }

    public readonly void WriteLight(IBufferWriter<byte> writer)
    {
        int bitsetSize = this.lightSections.Length + 2;
        BitSet skylightMask = new(bitsetSize);
        BitSet blocklightMask = new(bitsetSize);
        BitSet emptySkylightMask = new(bitsetSize);
        BitSet emptyBlocklightMask = new(bitsetSize);

        using PooledArrayBufferWriter<byte> skylightWriter = new(ArrayPool<byte>.Shared);
        using PooledArrayBufferWriter<byte> blocklightWriter = new(ArrayPool<byte>.Shared);

        int skylightSize = 0;
        int blocklightSize = 0;

        for (int i = 1; i < (bitsetSize - 1); ++i)
        {
            ref LightSection section = ref this.lightSections[i - 1];

            bool hasSkylight = section.HasSkylight;
            bool hasBlocklight = section.HasBlocklight;

            skylightMask[i] = hasSkylight;
            emptySkylightMask[i] = !hasSkylight;

            blocklightMask[i] = hasBlocklight;
            emptyBlocklightMask[i] = !hasBlocklight;

            if (hasSkylight)
            {
                ++skylightSize;

                section.TryWriteSkylight(skylightWriter);
            }

            if (hasBlocklight)
            {
                ++blocklightSize;

                section.TryWriteBlocklight(blocklightWriter);
            }
        }

        writer.WriteBitSet(skylightMask);
        writer.WriteBitSet(blocklightMask);
        writer.WriteBitSet(emptySkylightMask);
        writer.WriteBitSet(emptyBlocklightMask);

        writer.WriteVarInteger(skylightSize);
        writer.Write(skylightWriter.WrittenSpan);

        writer.WriteVarInteger(blocklightSize);
        writer.Write(blocklightWriter.WrittenSpan);
    }

    public void Dispose()
    {
        if (this.sections == null)
            return;

        for (int i = 0; i < this.sections.Length; ++i)
        {
            ref Section section = ref this.sections[i];
            section.Dispose();
        }

        this = default;
    }
}

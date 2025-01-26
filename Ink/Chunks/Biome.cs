using Ink.Chunks.Pallete;

namespace Ink.Chunks;

public readonly record struct BiomeStorage : IPalettedValue<BiomeStorage>
{
    public static byte MinIndirectBits
        => 1;

    public static byte MaxIndirectBits
        => 3;

    public static byte DirectBits
        => 6;

    public readonly int RegistryId;

    public int PaletteId
        => RegistryId;

    public BiomeStorage(int registryId)
        => RegistryId = registryId;

    public static BiomeStorage FromPaletteId(int id)
        => new(id);
}

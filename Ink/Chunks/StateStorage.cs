using Ink.Blocks.State;
using Ink.Chunks.Pallete;

namespace Ink.Chunks;

public readonly record struct StateStorage : IPalettedValue<StateStorage>
{
    public static byte MinIndirectBits
        => 4;

    public static byte MaxIndirectBits
        => 8;

    public static byte DirectBits
        => BlockStates.MaxStateBits;

    public readonly int RegistryId;

    public int PaletteId
        => RegistryId;

    public bool IsAir
         => RegistryId == BlockStates.Air.Root.Default.Id
         || RegistryId == BlockStates.CaveAir.Root.Default.Id
         || RegistryId == BlockStates.VoidAir.Root.Default.Id;

    public StateStorage(int registryId)
        => RegistryId = registryId;

    public static StateStorage FromPaletteId(int id)
        => new(id);

    public static implicit operator StateStorage(int value)
        => new(value);
}

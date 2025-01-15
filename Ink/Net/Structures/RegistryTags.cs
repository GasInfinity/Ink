using System.Buffers;
using Ink.Registries;

namespace Ink.Net.Structures;

public readonly record struct RegistryTags(Identifier RegistryId, RegistryTags.Entry[] Tags)
{
    public readonly record struct Entry(Identifier Tag, int[] Ids)
    {
        public readonly Identifier Tag = Tag;
        public readonly int[] Ids = Ids;
    }

    public readonly Identifier RegistryId = RegistryId;
    public readonly RegistryTags.Entry[] Tags = Tags;

    public void Write(IBufferWriter<byte> writer)
    {
        // TODO: This
    }

    public static bool TryRead(ReadOnlySpan<byte> data, out int bytesRead, out RegistryTags value)
    {
        value = default;
        bytesRead = default;
        return false;
    }
}

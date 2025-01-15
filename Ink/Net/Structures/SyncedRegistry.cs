using System.Buffers;
using Ink.Nbt;
using Ink.Nbt.Serialization;
using Ink.Registries;
using Ink.Util;
using Ink.Util.Extensions;

namespace Ink.Net.Structures;

public readonly record struct SyncedRegistry(Identifier Id, IReadOnlyRegistry Registry, bool DataSyncingNeeded)
{
    public readonly Identifier Id = Id;
    public readonly IReadOnlyRegistry Registry = Registry;
    public readonly bool DataSyncingNeeded = DataSyncingNeeded;

    public void Write(IBufferWriter<byte> writer)
    {
        Id.Write(writer);

        writer.WriteVarInteger(Registry.Count);
        foreach(Identifier key in Registry.Keys)
        {
            key.Write(writer);

            if(writer.TryWriteOptional(DataSyncingNeeded))
            {
                NbtSerializer.SerializeObject<JavaNbtDatatypeWriter>(writer, null, Registry.Get(key)!, InkNbtContext.DefaultNetworkOptions);  
            }
        }
    }

    public static bool TryRead(ReadOnlySpan<byte> data, out int bytesRead, out SyncedRegistry value)
    {
        value = default;
        bytesRead = default;
        return false;
    }
}

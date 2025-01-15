using Ink.Registries;
using Ink.Util;
using Ink.Util.Extensions;
using Rena.Native.Buffers.Extensions;
using Rena.Native.Extensions;
using System.Buffers;

namespace Ink.Items;

public readonly record struct ItemStack(int Count, int Id)
{
    public readonly int Count = Count;
    public readonly int Id = Id;

    public bool IsEmpty
        => Count <= 0;

    public bool TryMerge(ref ItemStack other, out ItemStack newStack)
    {
        if(Id != other.Id
        || Count >= 64
        || other.Count < 1)
        {
            newStack = this;
            return false;
        }

        int newCount = Count + other.Count;
        int remaining = newCount - 64;

        newStack = WithCount(int.Min(64, newCount));

        if(remaining >= 0)
        {
            other = new(Id, remaining);
        }
        else
        {
            other = default;
        }

        return true;
    }

    public ItemStack WithCount(int count)
        => count <= 0 ? default : new(count, Id);

    public Item? Item(FrozenRegistry<Item> itemRegistry)
        => itemRegistry.Get(Id);

    public bool Equals(ItemStack other)
        => Id == other.Id && Count == other.Count;

    public override int GetHashCode()
        => HashCode.Combine(Id, Count);

    public void Write(IBufferWriter<byte> writer)
    {
        writer.WriteVarInteger(Count);

        if(Count > 0)
        {
            writer.WriteVarInteger(Id);
            writer.WriteVarInteger(0); // TODO: COMPONENTS
            writer.WriteVarInteger(0);
        }
    }

    public static bool TryRead(ReadOnlySpan<byte> payload, out int bytesRead, out ItemStack result)
    {
        if(VarInteger<uint>.TryDecode(payload, out int countBytesRead, out uint count) != OperationStatus.Done)
        {
            bytesRead = default;
            result = default;
            return false;
        }
        payload = payload[countBytesRead..];

        if(count == 0)
        {
            bytesRead = countBytesRead;
            result = default;
            return true;
        }

        if(VarInteger<uint>.TryDecode(payload, out int idBytesRead, out uint id) != OperationStatus.Done)
        {
            bytesRead = default;
            result = default;
            return false;
        }
        payload = payload[idBytesRead..];

        if(VarInteger<uint>.TryDecode(payload, out int addedComponentsCountBytesRead, out uint addedComponents) != OperationStatus.Done)
        {
            bytesRead = default;
            result = default;
            return false;
        }
        payload = payload[addedComponentsCountBytesRead..];

        if(VarInteger<uint>.TryDecode(payload, out int removedComponentsCountBytesRead, out uint removedComponents) != OperationStatus.Done)
        {
            bytesRead = default;
            result = default;
            return false;
        }
        payload = payload[removedComponentsCountBytesRead..];

        result = new((int)count, (int)id);
        bytesRead = countBytesRead + idBytesRead + addedComponentsCountBytesRead + removedComponentsCountBytesRead;
        return true;
    }
}

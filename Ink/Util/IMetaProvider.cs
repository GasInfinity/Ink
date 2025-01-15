using System.Buffers;

namespace Ink.Util;

public interface IMetaProvider
{
    void WriteDirtyMetaAndClear(IBufferWriter<byte> writer);
    void WriteMeta(IBufferWriter<byte> writer);
}

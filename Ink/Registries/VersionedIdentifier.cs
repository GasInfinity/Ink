using System.Buffers;
using Ink.Util;
using Ink.Util.Extensions;

namespace Ink.Registries;

public readonly record struct VersionedIdentifier(string Namespace, string Path, string Version)
{
    public readonly string Namespace = Namespace;
    public readonly string Path = Path;
    public readonly string Version = Version;

    public void Write(IBufferWriter<byte> writer)
    {
        writer.WriteJUtf8String(Namespace);
        writer.WriteJUtf8String(Path);
        writer.WriteJUtf8String(Version);
    }

    public override string ToString()
        => ToString(null, null);

    public string ToString(string? format, IFormatProvider? formatProvider)
        => $"{Namespace}:{Path} ({Version})";

    public bool TryFormat(Span<char> destination, out int charsWritten, ReadOnlySpan<char> format, IFormatProvider? provider)
        => destination.TryWrite($"{Namespace}:{Path} ({Version})", out charsWritten);

    public static bool TryRead(ReadOnlySpan<byte> data, out int bytesRead, out VersionedIdentifier result)
    {
        if(JUtf8String.TryDecode(data, out int namespaceBytesRead, out string name) != OperationStatus.Done
        || JUtf8String.TryDecode(data.Slice(namespaceBytesRead), out int pathBytesRead, out string path) != OperationStatus.Done
        || JUtf8String.TryDecode(data.Slice(namespaceBytesRead + pathBytesRead), out int versionBytesRead, out string version) != OperationStatus.Done)
        {
            result = default;
            bytesRead = default;
            return false;
        }

        bytesRead = namespaceBytesRead + pathBytesRead + versionBytesRead;
        result = new(name, path, version);
        return true;
    }

    public static VersionedIdentifier Vanilla(string path, string version)
        => new(Identifier.DefaultNamespace, path, version);
}

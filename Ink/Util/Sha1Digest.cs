using System.Buffers;
using System.Diagnostics;
using System.Numerics;
using System.Security.Cryptography;
using Rena.Native.Buffers;

namespace Ink.Util;

public static class Sha1Digest
{
    const int HashSize = 20;
    const int MaxDigestSize = 80;

    public static string ComputeServer(ReadOnlySpan<char> serverId, ReadOnlySpan<byte> sharedSecret, ReadOnlySpan<byte> publicKey)
    {
        int totalLength = serverId.Length + sharedSecret.Length + publicKey.Length;
        using PooledArray<byte> pooledBuffer = new (ArrayPool<byte>.Shared, totalLength); 
        Span<byte> buffer = pooledBuffer.AsSpan().Slice(0, totalLength);

        for(int i = 0; i < serverId.Length; ++i)
            buffer[i] = (byte)(serverId[i] & 0b01111111);

        _ = sharedSecret.TryCopyTo(buffer.Slice(serverId.Length, sharedSecret.Length));
        _ = publicKey.TryCopyTo(buffer.Slice(serverId.Length + sharedSecret.Length, publicKey.Length));
        return Compute(buffer);
    }

    // Derived from: https://gist.github.com/ammaraskar/7b4a3f73bee9dc4136539644a0f27e63
    public static string Compute(ReadOnlySpan<byte> value)
    {
        using SHA1 sha = SHA1.Create();

        Span<byte> result = stackalloc byte[HashSize];
        _ = sha.TryComputeHash(value, result, out _);

        BigInteger brokenDigestInteger = new BigInteger(result, isBigEndian: true);
        bool negative = brokenDigestInteger < 0;

        if(negative)
            brokenDigestInteger = -brokenDigestInteger;

        Span<char> digestResult = stackalloc char[MaxDigestSize];
        bool success = brokenDigestInteger.TryFormat(digestResult, out int charsWritten, "x");

        if(!success)
            throw new UnreachableException($"Failed to hexdigest value {Convert.ToHexString(result)}");

        digestResult = digestResult[..charsWritten].TrimStart('0');

        if(negative)
            return string.Create(null, $"-{digestResult}");

        return digestResult.ToString();
    }
}

using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Engines;
using Org.BouncyCastle.Crypto.Modes;
using Org.BouncyCastle.Crypto.Parameters;

namespace Ink.Net.Encryption;

public sealed class EncryptionPair
{
    // TODO: Switch to System.Crypto when they implement non-allocating Span based API's
    private readonly IBlockCipher aesEngine;
    private readonly CfbBlockCipher cfbCipher;

    public EncryptionPair()
    {
        this.aesEngine = AesEngine_X86.IsSupported ? new AesEngine_X86() : new AesEngine(); 
        this.cfbCipher = new CfbBlockCipher(this.aesEngine, 8);
    }

    public void Init(bool encrypt, KeyParameter key, ParametersWithIV ivKey)
    {
        this.aesEngine.Init(encrypt, key); 
        this.cfbCipher.Init(encrypt, ivKey);
    }

    public void ProcessEntireBlock(ReadOnlySpan<byte> input, Span<byte> output)
    {
        Debug.Assert(input.Length <= output.Length, "Input must fit in the output");

        ref byte inCurrent = ref MemoryMarshal.GetReference(input);
        ref byte inEnd = ref Unsafe.Add(ref inCurrent, input.Length);

        ref byte outCurrent = ref MemoryMarshal.GetReference(output);

        while(Unsafe.IsAddressLessThan(ref inCurrent, ref inEnd))
        {
            _ = this.cfbCipher.ProcessBlock(MemoryMarshal.CreateReadOnlySpan(ref inCurrent, 1), MemoryMarshal.CreateSpan(ref outCurrent, 1));
            Unsafe.Add(ref inCurrent, 1);
            Unsafe.Add(ref outCurrent, 1);
        }
    }
}

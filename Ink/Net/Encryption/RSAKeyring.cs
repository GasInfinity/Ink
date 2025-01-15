using System.Security.Cryptography;

namespace Ink.Net.Encryption;

public readonly struct RSAKeyring
{
    public readonly RSA Keypair;
    public readonly byte[] PublicKey;

    public RSAKeyring(int size)
    {
        Keypair = RSA.Create(size);
        PublicKey = Keypair.ExportSubjectPublicKeyInfo();
    }
}

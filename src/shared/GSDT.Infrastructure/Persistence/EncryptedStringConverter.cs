using System.Security.Cryptography;
using System.Text;

namespace GSDT.Infrastructure.Persistence;

/// <summary>
/// EF Core value converter — AES-256-GCM application-level encryption for PII columns.
/// No SQL Server Enterprise or pgcrypto required.
///
/// Key lifecycle: 32-byte key loaded from Vault at startup (Phase 02 adds Vault integration).
/// Dev fallback: Encryption:FieldKey in appsettings.Development.json (base64 32 bytes).
///
/// Wire format (base64): [12-byte nonce][16-byte GCM tag][ciphertext]
///
/// Usage in entity config:
///   builder.Property(x => x.CitizenId).HasConversion(new EncryptedStringConverter(keyBytes));
/// </summary>
public sealed class EncryptedStringConverter : ValueConverter<string, string>
{
    public EncryptedStringConverter(byte[] keyBytes)
        : base(
            plaintext => Encrypt(plaintext, keyBytes),
            ciphertext => Decrypt(ciphertext, keyBytes))
    {
    }

    private static string Encrypt(string plaintext, byte[] key)
    {
        if (string.IsNullOrEmpty(plaintext)) return plaintext;

        var nonce = new byte[AesGcm.NonceByteSizes.MaxSize]; // 12 bytes
        RandomNumberGenerator.Fill(nonce);

        var plaintextBytes = Encoding.UTF8.GetBytes(plaintext);
        var ciphertext = new byte[plaintextBytes.Length];
        var tag = new byte[AesGcm.TagByteSizes.MaxSize]; // 16 bytes

        using var aesGcm = new AesGcm(key, tag.Length);
        aesGcm.Encrypt(nonce, plaintextBytes, ciphertext, tag);

        // Pack: nonce(12) + tag(16) + ciphertext
        var combined = new byte[nonce.Length + tag.Length + ciphertext.Length];
        Buffer.BlockCopy(nonce, 0, combined, 0, nonce.Length);
        Buffer.BlockCopy(tag, 0, combined, nonce.Length, tag.Length);
        Buffer.BlockCopy(ciphertext, 0, combined, nonce.Length + tag.Length, ciphertext.Length);

        return Convert.ToBase64String(combined);
    }

    private static string Decrypt(string encoded, byte[] key)
    {
        if (string.IsNullOrEmpty(encoded)) return encoded;

        var combined = Convert.FromBase64String(encoded);
        var nonceSize = AesGcm.NonceByteSizes.MaxSize;  // 12
        var tagSize = AesGcm.TagByteSizes.MaxSize;       // 16

        var nonce = combined[..nonceSize];
        var tag = combined[nonceSize..(nonceSize + tagSize)];
        var ciphertext = combined[(nonceSize + tagSize)..];
        var plaintext = new byte[ciphertext.Length];

        using var aesGcm = new AesGcm(key, tagSize);
        aesGcm.Decrypt(nonce, ciphertext, tag, plaintext);

        return Encoding.UTF8.GetString(plaintext);
    }
}

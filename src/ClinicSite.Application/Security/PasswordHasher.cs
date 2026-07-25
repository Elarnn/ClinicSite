using System.Security.Cryptography;

namespace ClinicSite.Application.Security;

/// <summary>
/// Password hashing using PBKDF2-HMAC-SHA256 (the .NET-recommended primitive via
/// <see cref="Rfc2898DeriveBytes"/>). Each password gets a fresh random salt; the stored value is
/// self-describing: <c>v1.{iterations}.{base64Salt}.{base64Hash}</c>, so the iteration count can be
/// raised later without breaking existing hashes. Verification is constant-time.
///
/// Only hashes are ever persisted — a plaintext password is never stored or transmitted.
/// </summary>
public static class PasswordHasher
{
    private const int SaltSizeBytes = 16;   // 128-bit salt
    private const int KeySizeBytes = 32;    // 256-bit derived key
    private const int Iterations = 210_000; // OWASP-range work factor for PBKDF2-SHA256
    private const char Separator = '.';
    private static readonly HashAlgorithmName Algorithm = HashAlgorithmName.SHA256;

    public static string Hash(string password)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(password);

        var salt = RandomNumberGenerator.GetBytes(SaltSizeBytes);
        var key = Rfc2898DeriveBytes.Pbkdf2(password, salt, Iterations, Algorithm, KeySizeBytes);

        return string.Join(Separator,
            "v1",
            Iterations.ToString(),
            Convert.ToBase64String(salt),
            Convert.ToBase64String(key));
    }

    public static bool Verify(string password, string? storedHash)
    {
        if (string.IsNullOrWhiteSpace(password) || string.IsNullOrWhiteSpace(storedHash))
        {
            return false;
        }

        var parts = storedHash.Split(Separator);
        if (parts.Length != 4 || parts[0] != "v1" || !int.TryParse(parts[1], out var iterations))
        {
            return false;
        }

        byte[] salt;
        byte[] expectedKey;
        try
        {
            salt = Convert.FromBase64String(parts[2]);
            expectedKey = Convert.FromBase64String(parts[3]);
        }
        catch (FormatException)
        {
            return false;
        }

        var actualKey = Rfc2898DeriveBytes.Pbkdf2(password, salt, iterations, Algorithm, expectedKey.Length);
        return CryptographicOperations.FixedTimeEquals(actualKey, expectedKey);
    }
}

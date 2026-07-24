using System.Security.Cryptography;
using System.Text;

namespace ClinicSite.Application.Security;

/// <summary>
/// Cryptographic helpers for booking confirmation / cancellation tokens.
///
/// A token is 256 bits of CSPRNG output, delivered to the patient as a Base64Url string inside
/// an e-mail link. The database only ever stores the lowercase-hex SHA-256 hash of that token,
/// so a database leak cannot be turned into working confirmation/cancellation links.
/// </summary>
public static class BookingTokens
{
    // 32 bytes == 256 bits of entropy.
    private const int TokenSizeBytes = 32;

    // The Base64Url encoding of 32 bytes is always 43 characters (no padding).
    private const int ExpectedTokenLength = 43;

    /// <summary>
    /// Generates a new random token. Returns the raw value (to be placed in an e-mail link, never
    /// stored) and its SHA-256 hash (safe to persist).
    /// </summary>
    public static (string Token, string Hash) Create()
    {
        var bytes = RandomNumberGenerator.GetBytes(TokenSizeBytes);
        var token = Base64UrlEncode(bytes);
        return (token, ComputeHash(token));
    }

    /// <summary>
    /// Validates the incoming token's shape and, if well-formed, computes its SHA-256 hash so it can
    /// be compared against a stored hash. Returns false for malformed input instead of throwing.
    /// </summary>
    public static bool TryComputeHash(string? token, out string hash)
    {
        hash = string.Empty;

        if (string.IsNullOrWhiteSpace(token) || token.Length != ExpectedTokenLength)
        {
            return false;
        }

        try
        {
            hash = ComputeHash(token);
            return true;
        }
        catch (FormatException)
        {
            return false;
        }
    }

    /// <summary>
    /// Constant-time comparison of two stored hashes (defence in depth on top of the indexed lookup).
    /// </summary>
    public static bool HashesEqual(string? a, string? b)
    {
        if (a is null || b is null)
        {
            return false;
        }

        var ba = Encoding.ASCII.GetBytes(a);
        var bb = Encoding.ASCII.GetBytes(b);
        return CryptographicOperations.FixedTimeEquals(ba, bb);
    }

    private static string ComputeHash(string token)
    {
        // Follow the token back to its raw bytes, then hash those bytes. Throws FormatException on
        // malformed Base64Url, which callers translate into a neutral "invalid link" response.
        var raw = Base64UrlDecode(token);
        var hash = SHA256.HashData(raw);
        return Convert.ToHexString(hash).ToLowerInvariant();
    }

    private static string Base64UrlEncode(byte[] bytes)
    {
        return Convert.ToBase64String(bytes)
            .TrimEnd('=')
            .Replace('+', '-')
            .Replace('/', '_');
    }

    private static byte[] Base64UrlDecode(string input)
    {
        var value = input.Replace('-', '+').Replace('_', '/');
        switch (value.Length % 4)
        {
            case 2: value += "=="; break;
            case 3: value += "="; break;
            case 1: throw new FormatException("Invalid Base64Url length.");
        }

        return Convert.FromBase64String(value);
    }
}

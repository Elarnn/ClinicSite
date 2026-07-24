using ClinicSite.Application.Security;

namespace ClinicSite.Tests;

public class BookingTokensTests
{
    [Fact]
    public void Create_produces_distinct_tokens_and_hashes()
    {
        var a = BookingTokens.Create();
        var b = BookingTokens.Create();

        Assert.NotEqual(a.Token, b.Token);
        Assert.NotEqual(a.Hash, b.Hash);
    }

    [Fact]
    public void Create_hash_matches_recomputed_hash_of_same_token()
    {
        var (token, hash) = BookingTokens.Create();

        Assert.True(BookingTokens.TryComputeHash(token, out var recomputed));
        Assert.Equal(hash, recomputed);
        Assert.Equal(64, hash.Length); // SHA-256 as lowercase hex
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("too-short")]
    [InlineData("contains invalid spaces and length that is definitely wrong here!!")]
    public void TryComputeHash_rejects_malformed_tokens(string token)
    {
        Assert.False(BookingTokens.TryComputeHash(token, out _));
    }

    [Fact]
    public void HashesEqual_is_true_only_for_identical_hashes()
    {
        var (token, hash) = BookingTokens.Create();
        BookingTokens.TryComputeHash(token, out var same);

        Assert.True(BookingTokens.HashesEqual(hash, same));
        Assert.False(BookingTokens.HashesEqual(hash, BookingTokens.Create().Hash));
    }
}

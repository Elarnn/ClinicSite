using ClinicSite.Application.Security;

namespace ClinicSite.Tests;

public class PasswordHasherTests
{
    [Fact]
    public void Hash_then_Verify_succeeds_for_the_right_password()
    {
        var hash = PasswordHasher.Hash("correct horse battery");
        Assert.True(PasswordHasher.Verify("correct horse battery", hash));
    }

    [Fact]
    public void Verify_fails_for_the_wrong_password()
    {
        var hash = PasswordHasher.Hash("correct horse battery");
        Assert.False(PasswordHasher.Verify("wrong password", hash));
    }

    [Fact]
    public void Hash_is_salted_so_two_hashes_of_the_same_password_differ()
    {
        var a = PasswordHasher.Hash("same-password");
        var b = PasswordHasher.Hash("same-password");

        Assert.NotEqual(a, b);
        Assert.True(PasswordHasher.Verify("same-password", a));
        Assert.True(PasswordHasher.Verify("same-password", b));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("not-a-valid-hash")]
    [InlineData("v1.210000.notbase64.notbase64")]
    public void Verify_returns_false_for_malformed_stored_hash(string? stored)
    {
        Assert.False(PasswordHasher.Verify("anything", stored));
    }
}

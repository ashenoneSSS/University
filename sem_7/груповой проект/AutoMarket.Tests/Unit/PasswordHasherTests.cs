using AutoMarket.Api.Services;
using FluentAssertions;

namespace AutoMarket.Tests.Unit;

public class PasswordHasherTests
{
    private readonly BCryptPasswordHasher _sut = new();

    [Fact]
    public void Hash_ThenVerify_Succeeds()
    {
        var hash = _sut.Hash("Admin123!");
        _sut.Verify("Admin123!", hash).Should().BeTrue();
        _sut.Verify("wrong", hash).Should().BeFalse();
    }

    [Fact]
    public void Hash_SamePassword_ProducesDifferentHashes()
    {
        _sut.Hash("same").Should().NotBe(_sut.Hash("same"));
    }

    [Fact]
    public void Verify_GarbageHash_ReturnsFalseInsteadOfThrowing()
    {
        _sut.Verify("x", "not-a-bcrypt-hash").Should().BeFalse();
    }
}

using AutoMarket.Api.Dtos;
using AutoMarket.Api.Exceptions;
using AutoMarket.Api.Services;
using AutoMarket.Tests.Helpers;
using FluentAssertions;
using Microsoft.Extensions.Options;

namespace AutoMarket.Tests.Unit;

public class AuthServiceTests : IDisposable
{
    private readonly TestDb _t = new();
    private readonly AuthService _sut;

    public AuthServiceTests()
    {
        var tokens = new TokenService(Options.Create(new JwtOptions { Key = new string('k', 40), Issuer = "test" }));
        _sut = new AuthService(_t.Db, tokens, _t.Hasher);
    }

    public void Dispose() => _t.Dispose();

    [Fact]
    public async Task Register_CreatesUserWithUserRole_AndReturnsToken()
    {
        var res = await _sut.RegisterAsync(new RegisterDto { Email = "new@test.com", Password = "123456", Name = "Ivan" });

        res.Token.Should().NotBeNullOrEmpty();
        res.Role.Should().Be("User");
        res.ExpiresAt.Should().BeAfter(DateTime.UtcNow);
        _t.Db.Users.Should().ContainSingle(u => u.Email == "new@test.com");
    }

    [Fact]
    public async Task Register_NormalizesEmail()
    {
        var res = await _sut.RegisterAsync(new RegisterDto { Email = "  MiXeD@Test.COM ", Password = "123456", Name = "A B" });
        res.Email.Should().Be("mixed@test.com");
    }

    [Fact]
    public async Task Register_DoesNotStorePlainPassword()
    {
        await _sut.RegisterAsync(new RegisterDto { Email = "p@test.com", Password = "123456", Name = "Pass" });
        _t.Db.Users.Single(u => u.Email == "p@test.com").PasswordHash.Should().NotBe("123456");
    }

    [Fact]
    public async Task Register_DuplicateEmail_Throws409()
    {
        await _t.AddUserAsync("dup@test.com");
        var act = () => _sut.RegisterAsync(new RegisterDto { Email = "DUP@test.com", Password = "123456", Name = "Dup" });
        await act.Should().ThrowAsync<ConflictException>();
    }

    [Fact]
    public async Task Login_CorrectPassword_ReturnsToken()
    {
        await _t.AddUserAsync("ok@test.com", password: "secret123");
        var res = await _sut.LoginAsync(new LoginDto { Email = "ok@test.com", Password = "secret123" });
        res.Token.Should().NotBeNullOrEmpty();
    }

    [Theory]
    [InlineData("ok@test.com", "wrong")]
    [InlineData("missing@test.com", "secret123")]
    public async Task Login_WrongCredentials_Throws401(string email, string password)
    {
        await _t.AddUserAsync("ok@test.com", password: "secret123");
        var act = () => _sut.LoginAsync(new LoginDto { Email = email, Password = password });
        await act.Should().ThrowAsync<UnauthorizedException>();
    }

    [Fact]
    public async Task Login_BlockedUser_Throws403()
    {
        await _t.AddUserAsync("blocked@test.com", password: "secret123", blocked: true);
        var act = () => _sut.LoginAsync(new LoginDto { Email = "blocked@test.com", Password = "secret123" });
        await act.Should().ThrowAsync<ForbiddenException>();
    }
}

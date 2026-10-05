using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using AutoMarket.Api.Models;
using AutoMarket.Api.Services;
using FluentAssertions;
using Microsoft.Extensions.Options;

namespace AutoMarket.Tests.Unit;

public class TokenServiceTests
{
    private readonly TokenService _sut = new(Options.Create(new JwtOptions { Key = new string('k', 40), Issuer = "test", ExpiresDays = 3 }));

    [Fact]
    public void Create_ContainsIdEmailAndRoleClaims()
    {
        var result = _sut.Create(new User { Id = 42, Email = "a@b.c", Name = "A", Role = UserRole.Admin });
        var jwt = new JwtSecurityTokenHandler().ReadJwtToken(result.Token);

        jwt.Claims.Should().Contain(c => c.Type == ClaimTypes.NameIdentifier && c.Value == "42");
        jwt.Claims.Should().Contain(c => c.Type == ClaimTypes.Email && c.Value == "a@b.c");
        jwt.Claims.Should().Contain(c => c.Type == ClaimTypes.Role && c.Value == "Admin");
        jwt.Issuer.Should().Be("test");
    }

    [Fact]
    public void Create_UsesConfiguredLifetime()
    {
        var result = _sut.Create(new User { Id = 1, Email = "a@b.c", Name = "A" });
        result.ExpiresAt.Should().BeCloseTo(DateTime.UtcNow.AddDays(3), TimeSpan.FromMinutes(1));
    }
}

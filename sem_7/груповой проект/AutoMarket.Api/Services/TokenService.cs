using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using AutoMarket.Api.Models;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace AutoMarket.Api.Services;

/// <summary>Налаштування JWT з секції "Jwt" appsettings.json.</summary>
public class JwtOptions
{
    public const string Section = "Jwt";
    public string Key { get; set; } = "";
    public string Issuer { get; set; } = "AutoMarket";
    public int ExpiresDays { get; set; } = 7;
}

public record TokenResult(string Token, DateTime ExpiresAt);

/// <summary>Видача JWT токенів.</summary>
public interface ITokenService
{
    TokenResult Create(User user);
}

public class TokenService(IOptions<JwtOptions> options) : ITokenService
{
    private readonly JwtOptions _opt = options.Value;

    public TokenResult Create(User user)
    {
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_opt.Key));
        var expires = DateTime.UtcNow.AddDays(_opt.ExpiresDays);
        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Email, user.Email),
            new Claim(ClaimTypes.Name, user.Name),
            new Claim(ClaimTypes.Role, user.Role.ToString())
        };
        var token = new JwtSecurityToken(_opt.Issuer, _opt.Issuer, claims, expires: expires,
            signingCredentials: new SigningCredentials(key, SecurityAlgorithms.HmacSha256));
        return new TokenResult(new JwtSecurityTokenHandler().WriteToken(token), expires);
    }
}

using AutoMarket.Api.Data;
using AutoMarket.Api.Dtos;
using AutoMarket.Api.Exceptions;
using AutoMarket.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace AutoMarket.Api.Services;

/// <summary>Реєстрація та вхід.</summary>
public interface IAuthService
{
    Task<AuthResponse> RegisterAsync(RegisterDto dto);
    Task<AuthResponse> LoginAsync(LoginDto dto);
}

public class AuthService(AppDbContext db, ITokenService tokens, IPasswordHasher hasher) : IAuthService
{
    public async Task<AuthResponse> RegisterAsync(RegisterDto dto)
    {
        var email = NormalizeEmail(dto.Email);
        if (await db.Users.AnyAsync(u => u.Email == email))
            throw new ConflictException("Користувач з таким email вже існує");

        var user = new User
        {
            Email = email,
            Name = dto.Name.Trim(),
            Phone = string.IsNullOrWhiteSpace(dto.Phone) ? null : dto.Phone.Trim(),
            PasswordHash = hasher.Hash(dto.Password)
        };
        db.Users.Add(user);
        await db.SaveChangesAsync();
        return BuildResponse(user);
    }

    public async Task<AuthResponse> LoginAsync(LoginDto dto)
    {
        var email = NormalizeEmail(dto.Email);
        var user = await db.Users.FirstOrDefaultAsync(u => u.Email == email);
        // однакова помилка для невідомого email і неправильного пароля — щоб не підказувати, які email існують
        if (user == null || !hasher.Verify(dto.Password, user.PasswordHash))
            throw new UnauthorizedException("Невірний email або пароль");
        if (user.IsBlocked)
            throw new ForbiddenException("Користувача заблоковано адміністратором");
        return BuildResponse(user);
    }

    private AuthResponse BuildResponse(User user)
    {
        var token = tokens.Create(user);
        return new AuthResponse(token.Token, token.ExpiresAt, user.Id, user.Email, user.Name, user.Role.ToString());
    }

    internal static string NormalizeEmail(string email) => email.Trim().ToLowerInvariant();
}

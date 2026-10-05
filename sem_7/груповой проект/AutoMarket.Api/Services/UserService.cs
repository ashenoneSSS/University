using AutoMarket.Api.Common;
using AutoMarket.Api.Data;
using AutoMarket.Api.Dtos;
using AutoMarket.Api.Exceptions;
using AutoMarket.Api.Mapping;
using AutoMarket.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace AutoMarket.Api.Services;

/// <summary>Профіль користувача та керування користувачами адміністратором.</summary>
public interface IUserService
{
    Task<UserDto> GetAsync(int id);
    Task<UserDto> UpdateProfileAsync(int userId, UpdateProfileDto dto);
    Task ChangePasswordAsync(int userId, ChangePasswordDto dto);

    Task<PagedResult<UserDto>> GetAllAsync(string? search, PageRequest page);
    Task<UserDto> SetBlockedAsync(int adminId, int userId, bool blocked);
    Task<UserDto> SetRoleAsync(int adminId, int userId, UserRole role);
    Task DeleteAsync(int adminId, int userId);
}

public class UserService(AppDbContext db, IPasswordHasher hasher, ILogger<UserService> logger) : IUserService
{
    public async Task<UserDto> GetAsync(int id)
    {
        var user = await FindAsync(id);
        return user.ToDto(await CountListingsAsync(id));
    }

    public async Task<UserDto> UpdateProfileAsync(int userId, UpdateProfileDto dto)
    {
        var user = await FindAsync(userId);
        user.Name = dto.Name.Trim();
        user.Phone = string.IsNullOrWhiteSpace(dto.Phone) ? null : dto.Phone.Trim();
        await db.SaveChangesAsync();
        return user.ToDto(await CountListingsAsync(userId));
    }

    public async Task ChangePasswordAsync(int userId, ChangePasswordDto dto)
    {
        var user = await FindAsync(userId);
        if (!hasher.Verify(dto.CurrentPassword, user.PasswordHash))
            throw new BadRequestException("Поточний пароль невірний");
        if (dto.CurrentPassword == dto.NewPassword)
            throw new BadRequestException("Новий пароль має відрізнятися від поточного");
        user.PasswordHash = hasher.Hash(dto.NewPassword);
        await db.SaveChangesAsync();
    }

    public async Task<PagedResult<UserDto>> GetAllAsync(string? search, PageRequest page)
    {
        var query = db.Users.AsNoTracking();
        if (!string.IsNullOrWhiteSpace(search))
        {
            // email вже зберігається в нижньому регістрі; ім'я порівнюємо як є + з великої літери (кирилиця в SQLite)
            var s = search.Trim();
            var lower = s.ToLowerInvariant();
            var capitalized = char.ToUpperInvariant(lower[0]) + lower[1..];
            query = query.Where(u => u.Email.Contains(lower) || u.Name.Contains(s) || u.Name.Contains(lower) || u.Name.Contains(capitalized));
        }

        var total = await query.CountAsync();
        var rows = await query.OrderBy(u => u.Id).Skip(page.Skip).Take(page.PageSize)
            .Select(u => new { User = u, Count = u.Listings.Count(l => l.Status != ListingStatus.Deleted) })
            .ToListAsync();

        return new PagedResult<UserDto>
        {
            Items = rows.Select(r => r.User.ToDto(r.Count)).ToList(),
            Page = page.Page,
            PageSize = page.PageSize,
            TotalCount = total
        };
    }

    public async Task<UserDto> SetBlockedAsync(int adminId, int userId, bool blocked)
    {
        if (adminId == userId)
            throw new BadRequestException("Не можна заблокувати самого себе");
        var user = await FindAsync(userId);
        user.IsBlocked = blocked;
        await db.SaveChangesAsync();
        logger.LogInformation("Адмін {AdminId}: користувач {UserId} blocked={Blocked}", adminId, userId, blocked);
        return user.ToDto(await CountListingsAsync(userId));
    }

    public async Task<UserDto> SetRoleAsync(int adminId, int userId, UserRole role)
    {
        if (adminId == userId && role != UserRole.Admin)
            throw new BadRequestException("Не можна зняти роль адміністратора з себе");
        var user = await FindAsync(userId);
        if (user.Role == UserRole.Admin && role != UserRole.Admin && await IsLastAdminAsync())
            throw new BadRequestException("У системі має лишитися хоча б один адміністратор");
        user.Role = role;
        await db.SaveChangesAsync();
        return user.ToDto(await CountListingsAsync(userId));
    }

    public async Task DeleteAsync(int adminId, int userId)
    {
        if (adminId == userId)
            throw new BadRequestException("Не можна видалити самого себе");
        var user = await FindAsync(userId);
        if (user.Role == UserRole.Admin && await IsLastAdminAsync())
            throw new BadRequestException("Не можна видалити останнього адміністратора");
        db.Users.Remove(user);
        await db.SaveChangesAsync();
        logger.LogWarning("Адмін {AdminId} видалив користувача {UserId}", adminId, userId);
    }

    private async Task<User> FindAsync(int id) =>
        await db.Users.FirstOrDefaultAsync(u => u.Id == id)
        ?? throw new NotFoundException($"Користувача {id} не знайдено");

    private Task<int> CountListingsAsync(int userId) =>
        db.Listings.CountAsync(l => l.UserId == userId && l.Status != ListingStatus.Deleted);

    private async Task<bool> IsLastAdminAsync() =>
        await db.Users.CountAsync(u => u.Role == UserRole.Admin) <= 1;
}

using System.ComponentModel.DataAnnotations;
using AutoMarket.Api.Models;

namespace AutoMarket.Api.Dtos;

/// <summary>Користувач у відповіді API (без хешу пароля).</summary>
public record UserDto(int Id, string Email, string Name, string? Phone, string Role, bool IsBlocked, DateTime CreatedAt, int ListingsCount);

/// <summary>Оновлення профілю.</summary>
public class UpdateProfileDto
{
    [Required, MinLength(2), MaxLength(100)]
    public string Name { get; set; } = "";

    [Phone, MaxLength(20)]
    public string? Phone { get; set; }
}

/// <summary>Зміна пароля.</summary>
public class ChangePasswordDto
{
    [Required]
    public string CurrentPassword { get; set; } = "";

    [Required, MinLength(6), MaxLength(100)]
    public string NewPassword { get; set; } = "";
}

/// <summary>Зміна ролі користувача адміністратором.</summary>
public class SetRoleDto
{
    [Required]
    public UserRole Role { get; set; }
}

using System.ComponentModel.DataAnnotations;

namespace AutoMarket.Api.Dtos;

/// <summary>Дані для реєстрації.</summary>
public class RegisterDto
{
    [Required, EmailAddress, MaxLength(256)]
    public string Email { get; set; } = "";

    [Required, MinLength(6), MaxLength(100)]
    public string Password { get; set; } = "";

    [Required, MinLength(2), MaxLength(100)]
    public string Name { get; set; } = "";

    [Phone, MaxLength(20)]
    public string? Phone { get; set; }
}

/// <summary>Дані для входу.</summary>
public class LoginDto
{
    [Required, EmailAddress]
    public string Email { get; set; } = "";

    [Required]
    public string Password { get; set; } = "";
}

/// <summary>Відповідь з JWT токеном.</summary>
public record AuthResponse(string Token, DateTime ExpiresAt, int UserId, string Email, string Name, string Role);

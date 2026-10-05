using System.ComponentModel.DataAnnotations;

namespace AutoMarket.Api.Dtos;

/// <summary>Дані для створення / редагування оголошення.</summary>
public class ListingCreateDto
{
    [Required, MinLength(3), MaxLength(150)]
    public string Title { get; set; } = "";

    [Required, MinLength(10), MaxLength(5000)]
    public string Description { get; set; } = "";

    /// <summary>Ціна в гривнях.</summary>
    [Range(0, 100_000_000)]
    public decimal Price { get; set; }

    [MaxLength(100)]
    public string? City { get; set; }

    [Range(1, int.MaxValue)]
    public int CategoryId { get; set; }

    /// <summary>Посилання на фото (до 10 шт., http/https).</summary>
    [MaxLength(10)]
    public List<string>? PhotoUrls { get; set; }
}

/// <summary>Причина відхилення оголошення.</summary>
public class RejectDto
{
    [Required, MinLength(3), MaxLength(500)]
    public string Reason { get; set; } = "";
}

/// <summary>Оголошення у відповіді API.</summary>
public record ListingDto(
    int Id,
    string Title,
    string Description,
    decimal Price,
    string? City,
    string Status,
    string? RejectReason,
    int CategoryId,
    string Category,
    int AuthorId,
    string Author,
    string? AuthorPhone,
    int Views,
    DateTime CreatedAt,
    DateTime? UpdatedAt,
    List<string> Photos);

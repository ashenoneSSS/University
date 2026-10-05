using System.ComponentModel.DataAnnotations;

namespace AutoMarket.Api.Dtos;

/// <summary>Категорія з кількістю активних оголошень.</summary>
public record CategoryDto(int Id, string Name, int ActiveListings);

/// <summary>Створення / перейменування категорії.</summary>
public class CategoryCreateDto
{
    [Required, MinLength(2), MaxLength(100)]
    public string Name { get; set; } = "";
}

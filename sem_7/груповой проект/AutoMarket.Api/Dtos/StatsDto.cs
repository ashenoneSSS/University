namespace AutoMarket.Api.Dtos;

/// <summary>Статистика для панелі адміністратора.</summary>
public record StatsDto(
    int TotalUsers,
    int BlockedUsers,
    int TotalListings,
    Dictionary<string, int> ListingsByStatus,
    List<CategoryDto> ActiveByCategory,
    int NewListingsLast7Days,
    decimal AverageActivePrice);

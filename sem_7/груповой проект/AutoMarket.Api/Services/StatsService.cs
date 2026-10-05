using AutoMarket.Api.Data;
using AutoMarket.Api.Dtos;
using AutoMarket.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace AutoMarket.Api.Services;

/// <summary>Агрегована статистика для адміністратора.</summary>
public interface IStatsService
{
    Task<StatsDto> GetAsync();
}

public class StatsService(AppDbContext db, ICategoryService categories) : IStatsService
{
    public async Task<StatsDto> GetAsync()
    {
        var byStatus = await db.Listings.GroupBy(l => l.Status)
            .Select(g => new { Status = g.Key, Count = g.Count() })
            .ToListAsync();

        var weekAgo = DateTime.UtcNow.AddDays(-7);
        // ціни рахуємо в пам'яті: SQLite не агрегує decimal
        var activePrices = await db.Listings.Where(l => l.Status == ListingStatus.Active).Select(l => l.Price).ToListAsync();

        return new StatsDto(
            TotalUsers: await db.Users.CountAsync(),
            BlockedUsers: await db.Users.CountAsync(u => u.IsBlocked),
            TotalListings: byStatus.Sum(x => x.Count),
            ListingsByStatus: Enum.GetValues<ListingStatus>()
                .ToDictionary(s => s.ToString(), s => byStatus.FirstOrDefault(x => x.Status == s)?.Count ?? 0),
            ActiveByCategory: await categories.GetAllAsync(),
            NewListingsLast7Days: await db.Listings.CountAsync(l => l.CreatedAt >= weekAgo),
            AverageActivePrice: activePrices.Count == 0 ? 0 : Math.Round(activePrices.Average(), 2));
    }
}

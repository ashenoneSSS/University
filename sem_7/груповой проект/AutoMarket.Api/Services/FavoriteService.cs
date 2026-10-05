using AutoMarket.Api.Data;
using AutoMarket.Api.Dtos;
using AutoMarket.Api.Exceptions;
using AutoMarket.Api.Mapping;
using AutoMarket.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace AutoMarket.Api.Services;

/// <summary>Обрані оголошення користувача.</summary>
public interface IFavoriteService
{
    Task<List<ListingDto>> GetAsync(int userId);
    Task AddAsync(int userId, int listingId);
    Task RemoveAsync(int userId, int listingId);
}

public class FavoriteService(AppDbContext db) : IFavoriteService
{
    public async Task<List<ListingDto>> GetAsync(int userId)
    {
        // показуємо лише ті, що досі активні
        var favorites = await db.Favorites.AsNoTracking()
            .Include(f => f.Listing).ThenInclude(l => l!.Category)
            .Include(f => f.Listing).ThenInclude(l => l!.User)
            .Include(f => f.Listing).ThenInclude(l => l!.Photos)
            .Where(f => f.UserId == userId && f.Listing!.Status == ListingStatus.Active)
            .OrderByDescending(f => f.CreatedAt)
            .ToListAsync();
        return favorites.Select(f => f.Listing!.ToDto()).ToList();
    }

    public async Task AddAsync(int userId, int listingId)
    {
        var listing = await db.Listings.FirstOrDefaultAsync(l => l.Id == listingId && l.Status == ListingStatus.Active)
                      ?? throw new NotFoundException($"Активне оголошення {listingId} не знайдено");
        if (listing.UserId == userId)
            throw new BadRequestException("Не можна додати власне оголошення в обране");
        if (await db.Favorites.AnyAsync(f => f.UserId == userId && f.ListingId == listingId))
            return; // ідемпотентно: повторне додавання не є помилкою
        db.Favorites.Add(new Favorite { UserId = userId, ListingId = listingId });
        await db.SaveChangesAsync();
    }

    public async Task RemoveAsync(int userId, int listingId)
    {
        var fav = await db.Favorites.FindAsync(userId, listingId)
                  ?? throw new NotFoundException("Оголошення немає в обраному");
        db.Favorites.Remove(fav);
        await db.SaveChangesAsync();
    }
}

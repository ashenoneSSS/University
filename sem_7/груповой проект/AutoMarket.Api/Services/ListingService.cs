using AutoMarket.Api.Common;
using AutoMarket.Api.Data;
using AutoMarket.Api.Dtos;
using AutoMarket.Api.Exceptions;
using AutoMarket.Api.Mapping;
using AutoMarket.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace AutoMarket.Api.Services;

/// <summary>Робота з оголошеннями: каталог, кабінет користувача, модерація.</summary>
public interface IListingService
{
    Task<PagedResult<ListingDto>> SearchAsync(ListingQuery query);
    Task<ListingDto> GetActiveAsync(int id);
    Task<List<ListingDto>> GetByUserAsync(int userId, ListingStatus? status);
    Task<ListingDto> GetOwnAsync(int userId, int id);
    Task<ListingDto> CreateAsync(int userId, ListingCreateDto dto);
    Task<ListingDto> UpdateAsync(int userId, int id, ListingCreateDto dto);
    Task DeleteAsync(int userId, int id);

    Task<PagedResult<ListingDto>> GetForModerationAsync(ListingStatus status, PageRequest page);
    Task<ListingDto> ApproveAsync(int id);
    Task<ListingDto> RejectAsync(int id, string reason);
    Task AdminDeleteAsync(int id);
}

public class ListingService(AppDbContext db, ILogger<ListingService> logger) : IListingService
{
    private const int MaxPhotos = 10;
    private const string LocalPhotosPath = "/images/listings/";

    private IQueryable<Listing> Full => db.Listings
        .Include(l => l.Category)
        .Include(l => l.User)
        .Include(l => l.Photos);

    public async Task<PagedResult<ListingDto>> SearchAsync(ListingQuery q)
    {
        if (q.MinPrice < 0 || q.MaxPrice < 0)
            throw new BadRequestException("Ціна не може бути від'ємною");
        if (q.MinPrice != null && q.MaxPrice != null && q.MinPrice > q.MaxPrice)
            throw new BadRequestException("minPrice не може бути більшим за maxPrice");

        var query = Full.AsNoTracking().Where(l => l.Status == ListingStatus.Active);

        if (!string.IsNullOrWhiteSpace(q.Q))
        {
            var text = Listing.Normalize(q.Q);
            query = query.Where(l => l.SearchText.Contains(text));
        }
        if (q.CategoryId != null) query = query.Where(l => l.CategoryId == q.CategoryId);
        if (q.MinPrice != null) query = query.Where(l => l.Price >= q.MinPrice);
        if (q.MaxPrice != null) query = query.Where(l => l.Price <= q.MaxPrice);
        if (!string.IsNullOrWhiteSpace(q.City))
        {
            var city = Listing.Normalize(q.City);
            query = query.Where(l => l.CityKey == city);
        }

        query = q.Sort switch
        {
            ListingSort.Oldest => query.OrderBy(l => l.CreatedAt),
            ListingSort.PriceAsc => query.OrderBy(l => l.Price).ThenByDescending(l => l.CreatedAt),
            ListingSort.PriceDesc => query.OrderByDescending(l => l.Price).ThenByDescending(l => l.CreatedAt),
            ListingSort.Popular => query.OrderByDescending(l => l.Views).ThenByDescending(l => l.CreatedAt),
            _ => query.OrderByDescending(l => l.CreatedAt)
        };

        return await ToPageAsync(query, q);
    }

    public async Task<ListingDto> GetActiveAsync(int id)
    {
        var l = await Full.FirstOrDefaultAsync(x => x.Id == id && x.Status == ListingStatus.Active)
                ?? throw new NotFoundException($"Оголошення {id} не знайдено");
        l.Views++;
        await db.SaveChangesAsync();
        return l.ToDto();
    }

    public async Task<List<ListingDto>> GetByUserAsync(int userId, ListingStatus? status)
    {
        var query = Full.AsNoTracking().Where(l => l.UserId == userId);
        query = status != null
            ? query.Where(l => l.Status == status)
            : query.Where(l => l.Status != ListingStatus.Deleted);
        var list = await query.OrderByDescending(l => l.CreatedAt).ToListAsync();
        return list.Select(l => l.ToDto()).ToList();
    }

    public async Task<ListingDto> GetOwnAsync(int userId, int id) =>
        (await FindOwnAsync(userId, id)).ToDto();

    public async Task<ListingDto> CreateAsync(int userId, ListingCreateDto dto)
    {
        await EnsureCategoryExistsAsync(dto.CategoryId);
        var listing = new Listing
        {
            UserId = userId,
            Status = ListingStatus.Pending,
            Photos = ToPhotos(dto.PhotoUrls)
        };
        Apply(listing, dto);
        db.Listings.Add(listing);
        await db.SaveChangesAsync();
        logger.LogInformation("Користувач {UserId} створив оголошення {ListingId}", userId, listing.Id);
        return (await Full.AsNoTracking().FirstAsync(x => x.Id == listing.Id)).ToDto();
    }

    public async Task<ListingDto> UpdateAsync(int userId, int id, ListingCreateDto dto)
    {
        var listing = await FindOwnAsync(userId, id);
        await EnsureCategoryExistsAsync(dto.CategoryId);

        Apply(listing, dto);
        db.Photos.RemoveRange(listing.Photos);
        listing.Photos = ToPhotos(dto.PhotoUrls);
        // будь-яка зміна повертає оголошення на модерацію
        listing.Status = ListingStatus.Pending;
        listing.RejectReason = null;
        listing.UpdatedAt = DateTime.UtcNow;

        await db.SaveChangesAsync();
        return (await Full.AsNoTracking().FirstAsync(x => x.Id == id)).ToDto();
    }

    public async Task DeleteAsync(int userId, int id)
    {
        var listing = await FindOwnAsync(userId, id);
        listing.Status = ListingStatus.Deleted;
        listing.UpdatedAt = DateTime.UtcNow;
        await db.SaveChangesAsync();
    }

    public async Task<PagedResult<ListingDto>> GetForModerationAsync(ListingStatus status, PageRequest page)
    {
        // черга модерації: старіші першими
        var query = Full.AsNoTracking().Where(l => l.Status == status).OrderBy(l => l.CreatedAt);
        return await ToPageAsync(query, page);
    }

    public async Task<ListingDto> ApproveAsync(int id)
    {
        var listing = await FindAnyAsync(id);
        if (listing.Status == ListingStatus.Deleted)
            throw new BadRequestException("Не можна схвалити видалене оголошення");
        listing.Status = ListingStatus.Active;
        listing.RejectReason = null;
        listing.UpdatedAt = DateTime.UtcNow;
        await db.SaveChangesAsync();
        logger.LogInformation("Оголошення {ListingId} схвалено", id);
        return listing.ToDto();
    }

    public async Task<ListingDto> RejectAsync(int id, string reason)
    {
        if (string.IsNullOrWhiteSpace(reason))
            throw new BadRequestException("Вкажіть причину відхилення");
        var listing = await FindAnyAsync(id);
        if (listing.Status == ListingStatus.Deleted)
            throw new BadRequestException("Не можна відхилити видалене оголошення");
        listing.Status = ListingStatus.Rejected;
        listing.RejectReason = reason.Trim();
        listing.UpdatedAt = DateTime.UtcNow;
        await db.SaveChangesAsync();
        logger.LogInformation("Оголошення {ListingId} відхилено: {Reason}", id, reason);
        return listing.ToDto();
    }

    public async Task AdminDeleteAsync(int id)
    {
        var listing = await FindAnyAsync(id);
        listing.Status = ListingStatus.Deleted;
        listing.UpdatedAt = DateTime.UtcNow;
        await db.SaveChangesAsync();
    }

    // ---------- допоміжні методи ----------

    private async Task<Listing> FindOwnAsync(int userId, int id)
    {
        var listing = await Full.FirstOrDefaultAsync(x => x.Id == id && x.Status != ListingStatus.Deleted)
                      ?? throw new NotFoundException($"Оголошення {id} не знайдено");
        if (listing.UserId != userId)
            throw new ForbiddenException("Це не ваше оголошення");
        return listing;
    }

    private async Task<Listing> FindAnyAsync(int id) =>
        await Full.FirstOrDefaultAsync(x => x.Id == id)
        ?? throw new NotFoundException($"Оголошення {id} не знайдено");

    private async Task EnsureCategoryExistsAsync(int categoryId)
    {
        if (!await db.Categories.AnyAsync(c => c.Id == categoryId))
            throw new BadRequestException($"Категорії {categoryId} не існує");
    }

    private static void Apply(Listing listing, ListingCreateDto dto)
    {
        listing.Title = dto.Title.Trim();
        listing.Description = dto.Description.Trim();
        listing.Price = dto.Price;
        listing.City = string.IsNullOrWhiteSpace(dto.City) ? null : dto.City.Trim();
        listing.CategoryId = dto.CategoryId;
    }

    private static List<Photo> ToPhotos(List<string>? urls)
    {
        var list = (urls ?? new()).Where(u => !string.IsNullOrWhiteSpace(u)).Select(u => u.Trim()).Distinct().ToList();
        if (list.Count > MaxPhotos)
            throw new BadRequestException($"Максимум {MaxPhotos} фото");
        foreach (var url in list)
        {
            // фото демо-оголошень лежать на нашому сервері (wwwroot/images/listings)
            if (url.StartsWith(LocalPhotosPath) && !url.Contains("..")) continue;
            if (!Uri.TryCreate(url, UriKind.Absolute, out var uri) || (uri.Scheme != Uri.UriSchemeHttp && uri.Scheme != Uri.UriSchemeHttps))
                throw new BadRequestException($"Некоректне посилання на фото: {url}");
        }
        return list.Select(u => new Photo { Url = u }).ToList();
    }

    private static async Task<PagedResult<ListingDto>> ToPageAsync(IQueryable<Listing> query, PageRequest page)
    {
        var total = await query.CountAsync();
        var items = await query.Skip(page.Skip).Take(page.PageSize).ToListAsync();
        return new PagedResult<ListingDto>
        {
            Items = items.Select(l => l.ToDto()).ToList(),
            Page = page.Page,
            PageSize = page.PageSize,
            TotalCount = total
        };
    }
}

using AutoMarket.Api.Data;
using AutoMarket.Api.Dtos;
using AutoMarket.Api.Exceptions;
using AutoMarket.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace AutoMarket.Api.Services;

/// <summary>Довідник категорій.</summary>
public interface ICategoryService
{
    Task<List<CategoryDto>> GetAllAsync();
    Task<CategoryDto> GetAsync(int id);
    Task<CategoryDto> CreateAsync(CategoryCreateDto dto);
    Task<CategoryDto> UpdateAsync(int id, CategoryCreateDto dto);
    Task DeleteAsync(int id);
}

public class CategoryService(AppDbContext db) : ICategoryService
{
    public async Task<List<CategoryDto>> GetAllAsync() =>
        await db.Categories.AsNoTracking().OrderBy(c => c.Name)
            .Select(c => new CategoryDto(c.Id, c.Name, c.Listings.Count(l => l.Status == ListingStatus.Active)))
            .ToListAsync();

    public async Task<CategoryDto> GetAsync(int id) =>
        await db.Categories.AsNoTracking().Where(c => c.Id == id)
            .Select(c => new CategoryDto(c.Id, c.Name, c.Listings.Count(l => l.Status == ListingStatus.Active)))
            .FirstOrDefaultAsync()
        ?? throw new NotFoundException($"Категорію {id} не знайдено");

    public async Task<CategoryDto> CreateAsync(CategoryCreateDto dto)
    {
        var name = dto.Name.Trim();
        await EnsureUniqueAsync(name, null);
        var category = new Category { Name = name };
        db.Categories.Add(category);
        await db.SaveChangesAsync();
        return new CategoryDto(category.Id, category.Name, 0);
    }

    public async Task<CategoryDto> UpdateAsync(int id, CategoryCreateDto dto)
    {
        var category = await db.Categories.FindAsync(id) ?? throw new NotFoundException($"Категорію {id} не знайдено");
        var name = dto.Name.Trim();
        await EnsureUniqueAsync(name, id);
        category.Name = name;
        await db.SaveChangesAsync();
        return await GetAsync(id);
    }

    public async Task DeleteAsync(int id)
    {
        var category = await db.Categories.FindAsync(id) ?? throw new NotFoundException($"Категорію {id} не знайдено");
        if (await db.Listings.AnyAsync(l => l.CategoryId == id))
            throw new ConflictException("Категорія містить оголошення, видалити неможливо");
        db.Categories.Remove(category);
        await db.SaveChangesAsync();
    }

    private async Task EnsureUniqueAsync(string name, int? exceptId)
    {
        // порівнюємо в пам'яті: SQLite lower() не працює з кирилицею, а категорій небагато
        var names = await db.Categories.Where(c => c.Id != exceptId).Select(c => c.Name).ToListAsync();
        if (names.Any(n => string.Equals(n, name, StringComparison.OrdinalIgnoreCase)))
            throw new ConflictException($"Категорія \"{name}\" вже існує");
    }
}

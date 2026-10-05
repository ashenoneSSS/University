using AutoMarket.Api.Data;
using AutoMarket.Api.Models;
using AutoMarket.Api.Services;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace AutoMarket.Tests.Helpers;

/// <summary>
/// Справжня SQLite у пам'яті: та сама СУБД, що й у застосунку, тож тести ловлять
/// проблеми трансляції LINQ → SQL, яких не видно з EF InMemory.
/// </summary>
public sealed class TestDb : IDisposable
{
    private readonly SqliteConnection _connection;
    public AppDbContext Db { get; }
    public IPasswordHasher Hasher { get; } = new FastHasher();

    public TestDb()
    {
        _connection = new SqliteConnection("DataSource=:memory:");
        _connection.Open();
        Db = new AppDbContext(new DbContextOptionsBuilder<AppDbContext>().UseSqlite(_connection).Options);
        Db.Database.EnsureCreated();
    }

    public async Task<User> AddUserAsync(string email = "user@test.com", UserRole role = UserRole.User, string password = "secret123", bool blocked = false)
    {
        var user = new User { Email = email, Name = email.Split('@')[0], Role = role, IsBlocked = blocked, PasswordHash = Hasher.Hash(password) };
        Db.Users.Add(user);
        await Db.SaveChangesAsync();
        return user;
    }

    public async Task<Listing> AddListingAsync(int userId, string title = "Test listing", decimal price = 1000,
        ListingStatus status = ListingStatus.Active, int categoryId = 1, string? city = "Київ", DateTime? createdAt = null)
    {
        var listing = new Listing
        {
            UserId = userId, Title = title, Description = "Опис оголошення для тесту", Price = price,
            Status = status, CategoryId = categoryId, City = city, CreatedAt = createdAt ?? DateTime.UtcNow
        };
        Db.Listings.Add(listing);
        await Db.SaveChangesAsync();
        return listing;
    }

    public void Dispose()
    {
        Db.Dispose();
        _connection.Dispose();
    }

    /// <summary>Швидкий «хешер» для юніт-тестів (BCrypt навмисно повільний).</summary>
    private sealed class FastHasher : IPasswordHasher
    {
        public string Hash(string password) => "hash:" + password;
        public bool Verify(string password, string hash) => hash == "hash:" + password;
    }
}

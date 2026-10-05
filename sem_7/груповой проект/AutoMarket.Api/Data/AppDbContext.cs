using AutoMarket.Api.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace AutoMarket.Api.Data;

/// <summary>Контекст EF Core (code-first). Схема змінюється через міграції.</summary>
public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<User> Users => Set<User>();
    public DbSet<Category> Categories => Set<Category>();
    public DbSet<Listing> Listings => Set<Listing>();
    public DbSet<Photo> Photos => Set<Photo>();
    public DbSet<Favorite> Favorites => Set<Favorite>();

    /// <summary>Перед збереженням автоматично оновлюємо поля пошуку в усіх змінених оголошеннях.</summary>
    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        RefreshListings();
        return base.SaveChangesAsync(cancellationToken);
    }

    public override int SaveChanges()
    {
        RefreshListings();
        return base.SaveChanges();
    }

    private void RefreshListings()
    {
        foreach (var entry in ChangeTracker.Entries<Listing>())
            if (entry.State is EntityState.Added or EntityState.Modified)
                entry.Entity.RefreshSearchFields();
    }

    protected override void OnModelCreating(ModelBuilder b)
    {
        b.Entity<User>(e =>
        {
            e.HasIndex(u => u.Email).IsUnique();
            e.Property(u => u.Email).HasMaxLength(256).IsRequired();
            e.Property(u => u.Name).HasMaxLength(100).IsRequired();
            e.Property(u => u.Phone).HasMaxLength(20);
            e.Property(u => u.Role).HasConversion<string>().HasMaxLength(20);
        });

        b.Entity<Category>(e =>
        {
            e.HasIndex(c => c.Name).IsUnique();
            e.Property(c => c.Name).HasMaxLength(100).IsRequired();
        });

        b.Entity<Listing>(e =>
        {
            e.Property(l => l.Title).HasMaxLength(150).IsRequired();
            e.Property(l => l.Description).HasMaxLength(5000).IsRequired();
            e.Property(l => l.City).HasMaxLength(100);
            e.Property(l => l.RejectReason).HasMaxLength(500);
            e.Property(l => l.Status).HasConversion<string>().HasMaxLength(20);
            // SQLite не вміє сортувати/порівнювати decimal, тому зберігаємо як double
            e.Property(l => l.Price).HasConversion<double>();
            e.Property(l => l.SearchText).HasMaxLength(5200).IsRequired();
            e.Property(l => l.CityKey).HasMaxLength(100);
            e.HasIndex(l => l.Status);
            e.HasIndex(l => l.CityKey);
            e.HasIndex(l => l.CategoryId);
            e.HasOne(l => l.Category).WithMany(c => c.Listings).HasForeignKey(l => l.CategoryId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        b.Entity<Photo>().Property(p => p.Url).HasMaxLength(1000).IsRequired();

        b.Entity<Favorite>(e =>
        {
            e.HasKey(f => new { f.UserId, f.ListingId });
            e.HasOne(f => f.User).WithMany(u => u.Favorites).HasForeignKey(f => f.UserId);
            e.HasOne(f => f.Listing).WithMany().HasForeignKey(f => f.ListingId);
        });

        // SQLite не зберігає DateTimeKind: позначаємо всі дати як UTC, щоб у JSON вони йшли з "Z"
        // і фронтенд коректно переводив їх у місцевий час
        var utc = new ValueConverter<DateTime, DateTime>(v => v, v => DateTime.SpecifyKind(v, DateTimeKind.Utc));
        var utcNullable = new ValueConverter<DateTime?, DateTime?>(v => v, v => v == null ? null : DateTime.SpecifyKind(v.Value, DateTimeKind.Utc));
        foreach (var property in b.Model.GetEntityTypes().SelectMany(t => t.GetProperties()))
        {
            if (property.ClrType == typeof(DateTime)) property.SetValueConverter(utc);
            else if (property.ClrType == typeof(DateTime?)) property.SetValueConverter(utcNullable);
        }

        b.Entity<Category>().HasData(
            new Category { Id = 1, Name = "Авто" },
            new Category { Id = 2, Name = "Мото" },
            new Category { Id = 3, Name = "Телефони" },
            new Category { Id = 4, Name = "Ноутбуки" },
            new Category { Id = 5, Name = "Побутова техніка" },
            new Category { Id = 6, Name = "Електроніка та фото" });
    }
}

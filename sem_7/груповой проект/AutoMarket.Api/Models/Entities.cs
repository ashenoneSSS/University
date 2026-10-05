namespace AutoMarket.Api.Models;

/// <summary>Роль користувача в системі.</summary>
public enum UserRole
{
    /// <summary>Звичайний користувач: створює та редагує свої оголошення.</summary>
    User,
    /// <summary>Адміністратор: модерує оголошення та керує користувачами.</summary>
    Admin
}

/// <summary>Життєвий цикл оголошення.</summary>
public enum ListingStatus
{
    /// <summary>Очікує модерації (після створення або редагування).</summary>
    Pending,
    /// <summary>Схвалене адміністратором, видно всім.</summary>
    Active,
    /// <summary>Відхилене адміністратором (з причиною).</summary>
    Rejected,
    /// <summary>Видалене (м'яке видалення, запис лишається в БД).</summary>
    Deleted
}

/// <summary>Зареєстрований користувач.</summary>
public class User
{
    public int Id { get; set; }
    public string Email { get; set; } = "";
    public string PasswordHash { get; set; } = "";
    public string Name { get; set; } = "";
    public string? Phone { get; set; }
    public UserRole Role { get; set; } = UserRole.User;
    public bool IsBlocked { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public List<Listing> Listings { get; set; } = new();
    public List<Favorite> Favorites { get; set; } = new();
}

/// <summary>Категорія оголошень (Авто, Телефони, ...).</summary>
public class Category
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
    public List<Listing> Listings { get; set; } = new();
}

/// <summary>Оголошення про продаж б/у техніки чи авто.</summary>
public class Listing
{
    public int Id { get; set; }
    public string Title { get; set; } = "";
    public string Description { get; set; } = "";
    public decimal Price { get; set; }
    public string? City { get; set; }
    public ListingStatus Status { get; set; } = ListingStatus.Pending;
    public string? RejectReason { get; set; }
    public int Views { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }

    /// <summary>
    /// Назва + опис у нижньому регістрі для пошуку. SQLite-функція lower() працює лише з латиницею,
    /// тому нормалізуємо в C# при збереженні (див. <see cref="Normalize"/>).
    /// </summary>
    public string SearchText { get; set; } = "";

    /// <summary>Місто в нижньому регістрі для фільтра.</summary>
    public string? CityKey { get; set; }

    public int UserId { get; set; }
    public User? User { get; set; }
    public int CategoryId { get; set; }
    public Category? Category { get; set; }
    public List<Photo> Photos { get; set; } = new();

    /// <summary>Оновлює службові поля пошуку. Викликати після зміни назви, опису або міста.</summary>
    public void RefreshSearchFields()
    {
        SearchText = Normalize($"{Title} {Description}");
        CityKey = string.IsNullOrWhiteSpace(City) ? null : Normalize(City);
    }

    public static string Normalize(string value) => value.Trim().ToLowerInvariant();
}

/// <summary>Фото оголошення (зберігаємо лише URL).</summary>
public class Photo
{
    public int Id { get; set; }
    public string Url { get; set; } = "";
    public int ListingId { get; set; }
    public Listing? Listing { get; set; }
}

/// <summary>Оголошення в обраному користувача.</summary>
public class Favorite
{
    public int UserId { get; set; }
    public User? User { get; set; }
    public int ListingId { get; set; }
    public Listing? Listing { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

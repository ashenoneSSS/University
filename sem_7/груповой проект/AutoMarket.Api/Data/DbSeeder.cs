using AutoMarket.Api.Models;
using AutoMarket.Api.Services;
using Microsoft.EntityFrameworkCore;

namespace AutoMarket.Api.Data;

/// <summary>Початкове заповнення БД: адміністратор і демо-оголошення.</summary>
public static class DbSeeder
{
    public const string AdminEmail = "admin@automarket.local";
    public const string AdminPassword = "Admin123!";

    /// <summary>Застосовує міграції та створює дані, якщо їх ще немає.</summary>
    public static async Task SeedAsync(AppDbContext db, IPasswordHasher hasher, bool withDemoData)
    {
        await db.Database.MigrateAsync();

        if (!await db.Users.AnyAsync(u => u.Role == UserRole.Admin))
        {
            db.Users.Add(new User
            {
                Email = AdminEmail, Name = "Admin", Role = UserRole.Admin,
                PasswordHash = hasher.Hash(AdminPassword)
            });
            await db.SaveChangesAsync();
        }

        if (!withDemoData || await db.Listings.AnyAsync()) return;

        var sellers = new[]
        {
            new User { Email = "demo@automarket.local", Name = "Демо Продавець", Phone = "+380501234567", PasswordHash = hasher.Hash("Demo123!") },
            new User { Email = "olena@automarket.local", Name = "Олена Коваленко", Phone = "+380671112233", PasswordHash = hasher.Hash("Demo123!") },
            new User { Email = "andriy@automarket.local", Name = "Андрій Мельник", Phone = "+380931234455", PasswordHash = hasher.Hash("Demo123!") },
            new User { Email = "autosalon@automarket.local", Name = "АвтоСалон «Схід»", Phone = "+380442001020", PasswordHash = hasher.Hash("Demo123!") }
        };
        db.Users.AddRange(sellers);

        var random = new Random(42); // фіксоване зерно — однакові дані при кожному створенні БД
        var now = DateTime.UtcNow;
        for (var i = 0; i < DemoListings.Length; i++)
        {
            var d = DemoListings[i];
            db.Listings.Add(new Listing
            {
                Title = d.Title,
                Description = d.Description,
                Price = d.Price,
                City = d.City,
                CategoryId = d.CategoryId,
                Status = d.Status,
                RejectReason = d.Status == ListingStatus.Rejected ? "Фото не відповідає опису, додайте реальні фото товару" : null,
                // авто частіше продає автосалон, решту — приватні продавці по черзі
                User = sellers[d.CategoryId == 1 && i % 2 == 0 ? 3 : i % 3],
                Views = d.Status == ListingStatus.Active ? random.Next(5, 600) : 0,
                CreatedAt = now.AddDays(-random.Next(0, 30)).AddMinutes(-random.Next(0, 1440)),
                Photos = { new Photo { Url = $"/images/listings/{d.Photo}.jpg" } }
            });
        }
        await db.SaveChangesAsync();
    }

    private record DemoListing(string Title, string Description, decimal Price, string City, int CategoryId, string Photo,
        ListingStatus Status = ListingStatus.Active);

    // Фото: Wikimedia Commons (вільні ліцензії), див. wwwroot/images/listings/SOURCES.md
    private static readonly DemoListing[] DemoListings =
    {
        // ---- Авто (1) ----
        new("Toyota Camry Hybrid 2012", "Гібрид, витрата 5.5 л/100 км. Один власник, обслуговування на офіційному СТО, всі чеки.", 485000, "Київ", 1, "camry"),
        new("Volkswagen Golf VII 1.4 TSI", "2015 рік, пробіг 142 тис. км, DSG, підігрів сидінь, парктроніки. Не фарбований.", 395000, "Львів", 1, "golf"),
        new("Skoda Octavia Combi III", "Універсал 2017 року, 2.0 TDI, механіка. Економна та містка, ідеальна для сім'ї.", 420000, "Вінниця", 1, "octavia"),
        new("BMW X5 M Competition", "2020 рік, повна комплектація, панорама, Harman Kardon. Пробіг 58 тис. км.", 3450000, "Київ", 1, "bmw-x5"),
        new("Ford Focus ST Mk3", "2.0 EcoBoost, 250 к.с. Жовтий, рекаро, ксенон. Без ДТП, свіже ТО.", 355000, "Дніпро", 1, "focus"),
        new("Daewoo Lanos 1.5", "2008 рік, газ/бензин. Надійна робоча машина, кондиціонер працює. Торг біля капота.", 95000, "Запоріжжя", 1, "lanos"),
        new("Tesla Model 3 Long Range", "2023 рік, запас ходу 560 км, автопілот. Батарея 98%. Червоний металік.", 1350000, "Одеса", 1, "tesla3"),
        new("Toyota RAV4 XLE AWD", "2016 рік, повний привід, 2.5 бензин, автомат. Доглянутий, зимова гума в подарунок.", 640000, "Харків", 1, "rav4"),
        new("Mercedes-Benz E-Class W212", "E200, 2013 рік, чорний, бежевий салон. Пневмопідвіска, навігація.", 560000, "Київ", 1, "mercedes-e", ListingStatus.Pending),

        // ---- Мото (2) ----
        new("Yamaha YZF-R6", "Спортбайк 2008 року, 600 куб. Сезон відкатаний без проблем, нова ланцюгова передача.", 210000, "Дніпро", 2, "r6"),
        new("Honda CBR600RR", "Помаранчевий, 2011 рік, ABS. Слайдери, глушник Akrapovič, документи в порядку.", 245000, "Київ", 2, "cbr"),
        new("Vespa Primavera 125", "Скутер для міста, пробіг 6 тис. км. Ідеальний стан, кофр у подарунок.", 120000, "Львів", 2, "vespa"),
        new("Harley-Davidson Sportster 883", "Класичний круізер 2014 року, багато хрому, звук — пісня. Обмін не цікавить.", 330000, "Одеса", 2, "harley"),
        new("Велосипед Merida Big.Seven 27.5", "Гірський хардтейл, рама M, гідравлічні гальма Shimano. Катався один сезон.", 18500, "Івано-Франківськ", 2, "bike"),

        // ---- Телефони (3) ----
        new("iPhone 13 Pro 256GB Alpine Green", "Батарея 88%, Face ID працює, без ремонтів. Повний комплект з коробкою.", 26500, "Київ", 3, "iphone13"),
        new("Samsung Galaxy S20 128GB", "Різні кольори в наявності. Екран без вигорань, чохол і скло в подарунок.", 9800, "Харків", 3, "galaxy-s20"),
        new("Google Pixel 7 Pro", "Найкраща камера в класі, чистий Android. Невеликі потертості на рамці.", 17900, "Львів", 3, "pixel"),
        new("iPhone 11 (PRODUCT)RED 64GB", "Червоний, батарея 81%. Підійде як перший айфон для дитини.", 9500, "Полтава", 3, "iphone11"),
        new("Xiaomi Redmi Note 10 Pro", "108 Мп камера, AMOLED 120 Гц. Глобальна версія, українська мова.", 6200, "Чернігів", 3, "xiaomi"),
        new("Nokia 3310 (оригінал)", "Легенда! Тримає заряд тиждень, змійка на місці. Для колекції або як запасний.", 1200, "Суми", 3, "nokia3310"),

        // ---- Ноутбуки (4) ----
        new("MacBook Air M1 8/256", "2020 рік, 74 цикли батареї, стан нового. Ідеально для навчання.", 23500, "Одеса", 4, "macbook-air"),
        new("MacBook Pro 15\" 2017 i7/16/512", "Touch Bar, Radeon Pro 555. Батарею замінено в сервісі пів року тому.", 21000, "Київ", 4, "macbook-pro"),
        new("Lenovo ThinkPad X1 Carbon", "Легкий бізнес-ноутбук, 14\", i5/8/256, підсвітка клавіатури. Windows 11 Pro.", 11500, "Житомир", 4, "thinkpad"),
        new("Dell XPS 13 та XPS 15", "Продаю два ноутбуки разом або окремо. Обидва в гарному стані, ціна за 13\".", 19500, "Київ", 4, "dell-xps"),
        new("ASUS ROG Zephyrus G14", "Ryzen 9, RTX 4060, 32 ГБ RAM. Ігровий ноутбук у компактному корпусі.", 48000, "Дніпро", 4, "asus-rog"),
        new("Lenovo ThinkPad X61 Tablet", "Ноутбук-трансформер з поворотним екраном і стилусом. Для ретро-колекціонерів.", 2500, "Луцьк", 4, "thinkpad-x61"),

        // ---- Побутова техніка (5) та Електроніка (6) ----
        new("Пральна машина Whirlpool 7 кг", "Фронтальне завантаження, 1200 об/хв, клас A++. Працює тихо, без протікань.", 7800, "Київ", 5, "washer"),
        new("Холодильник LG No Frost", "Двокамерний, 384 л, інверторний компресор. Причина продажу — переїзд.", 14500, "Львів", 5, "fridge"),
        new("Мікрохвильова піч 20 л", "Механічне керування, 700 Вт. Чиста, всередині без подряпин.", 1400, "Рівне", 5, "microwave"),
        new("Пилосос Dyson DC14", "Вертикальний, без мішка, циклонна технологія. Пройшов чистку та обслуговування.", 3900, "Тернопіль", 5, "dyson"),
        new("Кавоварка De'Longhi Dedica", "Ріжкова кавоварка, є капучинатор. Три кольори в наявності.", 6500, "Одеса", 5, "coffee"),
        new("Телевізор Samsung LED 55\"", "4K, Smart TV, HDR. Пульт і підставка в комплекті. Без битих пікселів.", 12000, "Харків", 6, "tv"),
        new("Посудомийна машина вбудована 60 см", "12 комплектів, 5 програм, половинне завантаження. Знята з кухні після ремонту.", 8200, "Київ", 5, "dishwasher"),
        new("Sony PlayStation 3 Slim + геймпад", "320 ГБ, прошита, 15 ігор встановлено. Ретро-гейминг за копійки.", 3500, "Кривий Ріг", 6, "ps3"),
        new("Навушники Apple AirPods Pro", "Шумозаглушення працює, кейс з бездротовою зарядкою. Нові амбушури.", 4800, "Ужгород", 6, "airpods", ListingStatus.Pending),
        new("Фотоапарат Canon EOS 5D Mark III", "Повнокадрова дзеркалка, пробіг затвора 42 тис. Тільки body.", 32000, "Київ", 6, "canon", ListingStatus.Rejected)
    };
}

# AutoMarket — дошка оголошень б/у техніки та авто

REST API на ASP.NET Core 8 + EF Core (code-first, міграції) + SQLite, JWT з ролями User/Admin, Swagger, xUnit-тести.
Фронтенд — Angular 22 + TypeScript.

## Вимоги
- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0) — `dotnet --version` має показати 8.x
- [Node.js](https://nodejs.org) 20+ (macOS: `brew install node`) — лише для фронтенду
- JetBrains Rider (необов'язково, можна з терміналу)

## Запуск

### 1. API (обов'язково)
**Rider:** відкрити `AutoMarket.sln` → вгорі вибрати конфігурацію `AutoMarket.Api: http` → Run ▶.

**Термінал:**
```bash
cd AutoMarket.Api
dotnet run --launch-profile http
```
Swagger: http://localhost:5299/swagger. БД `automarket.db` створюється сама при першому запуску
(міграції + сид-дані: адмін, 4 продавці, 6 категорій, 36 демо-оголошень із фото).

### 2. Вебінтерфейс (API має бути запущений)
```bash
cd AutoMarket.Web
npm install   # лише перший раз
npm start     # відкриє http://localhost:4200
```
У Rider: після `npm install` можна запускати конфігурацію `AutoMarket.Web` → Run ▶.

### 3. Тести
```bash
dotnet test
```
або в Rider: правий клік на `AutoMarket.Tests` → Run Unit Tests (109 тестів).

### Скинути базу до початкового стану
Зупинити API, видалити `AutoMarket.Api/automarket.db*` і запустити знову.

## Облікові записи
| Роль | Email | Пароль |
|---|---|---|
| Admin | `admin@automarket.local` | `Admin123!` |
| User (демо) | `demo@automarket.local` | `Demo123!` |
| Інші продавці | `olena@automarket.local`, `andriy@automarket.local`, `autosalon@automarket.local` | `Demo123!` |

У Swagger: `POST /api/auth/login` → скопіювати `token` → **Authorize** → вставити.

## Документація
- ER-діаграма БД та схема архітектури — [docs/DIAGRAMS.md](docs/DIAGRAMS.md) (PNG/SVG у `docs/diagrams/`)
- Звіт — `docs/AutoMarket_звіт.docx` (+ PDF)
- Презентація — `docs/AutoMarket_презентація.pptx` (+ PDF)

## Структура
```
AutoMarket.Api/
  Controllers/          Auth, Listings, Categories, Favorites, Profile
  Controllers/Admin/    AdminListings (модерація), AdminUsers, AdminStats
  Services/             бізнес-логіка (інтерфейс + реалізація для кожного)
  Middleware/           обробка помилок, логування запитів, перевірка заблокованих
  Models/  Dtos/  Mapping/  Common/  Exceptions/  Extensions/
  Data/                 AppDbContext, DbSeeder
  Migrations/           міграції EF Core
AutoMarket.Web/          Angular-фронтенд (core/, shared/, pages/, pages/admin/)
AutoMarket.Tests/
  Unit/                 тести сервісів на SQLite in-memory
  Integration/          HTTP-тести всього API (WebApplicationFactory)
```

## API
| Хто | Метод | Опис |
|---|---|---|
| Гість | `GET /api/listings` | каталог: `q, categoryId, minPrice, maxPrice, city, sort, page, pageSize` |
| Гість | `GET /api/listings/{id}` | оголошення (+лічильник переглядів) |
| Гість | `GET /api/categories`, `/{id}` | категорії з кількістю оголошень |
| Гість | `POST /api/auth/register`, `/login` | реєстрація / вхід |
| User | `GET /api/listings/my`, `/my/{id}` | свої оголошення та статуси |
| User | `POST /api/listings`, `PUT /{id}`, `DELETE /{id}` | створити / редагувати (→ Pending) / видалити |
| User | `GET/POST/DELETE /api/favorites/{id}` | обране |
| User | `GET/PUT /api/profile`, `POST /api/profile/change-password` | профіль |
| Admin | `GET /api/admin/listings?status=Pending` | черга модерації |
| Admin | `POST /api/admin/listings/{id}/approve`, `/reject`, `DELETE /{id}` | модерація |
| Admin | `GET /api/admin/users`, `/{id}`, `/{id}/listings` | користувачі |
| Admin | `POST /{id}/block`, `/unblock`, `PUT /{id}/role`, `DELETE /{id}` | керування користувачами |
| Admin | `POST/PUT/DELETE /api/categories` | категорії |
| Admin | `GET /api/admin/stats` | статистика |
| — | `GET /health` | перевірка стану (БД) |

## Що реалізовано
- Статуси оголошень: Pending → Active / Rejected, редагування повертає в Pending, видалення м'яке (Deleted).
- Валідація DTO (DataAnnotations) + бізнес-перевірки в сервісах, помилки у форматі ProblemDetails.
- Захист: BCrypt-хеші, JWT, ролі, блокування діє одразу (навіть зі старим токеном), rate limit на /auth.
- Адмін не може заблокувати/видалити/понизити себе, не можна прибрати останнього адміністратора.
- Пагінація, сортування, регістронезалежний пошук (включно з кирилицею).
- XML-документація відображається в Swagger.

## Нова міграція
```bash
dotnet tool restore
cd AutoMarket.Api && dotnet ef migrations add Назва
```

## PostgreSQL замість SQLite
`dotnet add package Npgsql.EntityFrameworkCore.PostgreSQL -v 8.0.11`, у `Extensions/ServiceCollectionExtensions.cs` замінити `UseSqlite` на `UseNpgsql`,
змінити рядок підключення в `appsettings.json`, видалити `Migrations/` і створити міграцію заново.

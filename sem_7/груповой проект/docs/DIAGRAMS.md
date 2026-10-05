# Діаграми AutoMarket

Архітектура — текст Mermaid у `diagrams/architecture.mmd` (Rider і GitHub малюють його прямо в Markdown), ER-діаграма — скрипт `diagrams/er.py`. Готові картинки для звіту/презентації: `diagrams/*.png` і `diagrams/*.svg`.

## ER-діаграма бази даних

5 таблиць: користувачі, категорії, оголошення, фото, обране (зв'язок багато-до-багатьох між користувачами та оголошеннями).
Показано колонки з типами, первинні (PK) і зовнішні (FK) ключі, навігаційні властивості EF Core та зв'язки 1:N.

![ER-діаграма](diagrams/er.png)

Картинка генерується скриптом: `python3 docs/diagrams/er.py` → `er.svg`.

## Архітектура

Клієнт-серверна архітектура: Angular SPA → REST API (ASP.NET Core) → EF Core → SQLite. API розділено на шари: middleware → контролери → сервіси → доступ до даних.

```mermaid
flowchart TB
    user(["Користувач / Адмін<br/>(браузер)"])

    subgraph WEB["AutoMarket.Web — Angular 22 (SPA), :4200"]
        direction LR
        pages["Pages<br/>Home · ListingDetails · Login · Register<br/>MyListings · ListingForm · Favorites<br/>Admin: Moderation · Users · Stats"]
        guards["Route Guards<br/>auth · admin · guest"]
        authsvc["AuthService<br/>JWT у localStorage"]
        apisvc["ApiService<br/>HttpClient"]
        interceptor["AuthInterceptor<br/>Authorization: Bearer"]
        pages --> guards
        pages --> apisvc
        guards --> authsvc
        apisvc --> interceptor
    end

    subgraph API["AutoMarket.Api — ASP.NET Core 8 Web API, :5299"]
        direction LR
        mw["Middleware pipeline<br/>RequestLogging → ExceptionHandling → CORS<br/>→ RateLimiter → JWT Authentication<br/>→ BlockedUser → Authorization"]
        ctrl["Controllers (REST)<br/>Auth · Listings · Categories · Favorites · Profile<br/>Admin: Listings · Users · Stats"]
        svc["Services (бізнес-логіка)<br/>Auth · Listing · User · Category<br/>Favorite · Stats · Token · PasswordHasher"]
        ef["EF Core 8<br/>AppDbContext + Migrations + DbSeeder"]
        static["Static files<br/>wwwroot/images"]
        swagger["Swagger UI<br/>/swagger"]
        health["Health check<br/>/health"]
        mw --> ctrl --> svc --> ef
    end

    db[("SQLite<br/>automarket.db")]

    subgraph TESTS["AutoMarket.Tests — xUnit"]
        unit["Unit-тести сервісів<br/>(SQLite in-memory)"]
        integ["Інтеграційні HTTP-тести<br/>(WebApplicationFactory)"]
    end

    user --> pages
    interceptor -- "HTTP/JSON + JWT" --> mw
    pages -. "фото" .-> static
    ef --> db
    unit -.-> svc
    integ -.-> mw
    user -. "тестування API" .-> swagger
```

![Архітектура](diagrams/architecture.png)

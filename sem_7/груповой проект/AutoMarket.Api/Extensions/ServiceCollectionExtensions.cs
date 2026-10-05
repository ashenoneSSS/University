using System.Reflection;
using System.Text;
using System.Threading.RateLimiting;
using AutoMarket.Api.Data;
using AutoMarket.Api.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;

namespace AutoMarket.Api.Extensions;

/// <summary>Реєстрація сервісів у DI-контейнері, щоб Program.cs лишався коротким.</summary>
public static class ServiceCollectionExtensions
{
    public const string AuthRateLimitPolicy = "auth";
    public const string CorsPolicy = "frontend";

    /// <summary>БД + бізнес-сервіси.</summary>
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        // рядок підключення читається ліниво, щоб тести могли підмінити конфігурацію
        services.AddDbContext<AppDbContext>((sp, o) =>
            o.UseSqlite(sp.GetRequiredService<IConfiguration>().GetConnectionString("Default")));

        services.AddSingleton<IPasswordHasher, BCryptPasswordHasher>();
        services.AddScoped<ITokenService, TokenService>();
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<IListingService, ListingService>();
        services.AddScoped<IUserService, UserService>();
        services.AddScoped<ICategoryService, CategoryService>();
        services.AddScoped<IFavoriteService, FavoriteService>();
        services.AddScoped<IStatsService, StatsService>();
        return services;
    }

    /// <summary>JWT-автентифікація з перевіркою підпису, видавця та терміну дії.</summary>
    public static IServiceCollection AddJwtAuth(this IServiceCollection services, IConfiguration config)
    {
        var section = config.GetSection(JwtOptions.Section);
        services.Configure<JwtOptions>(section);
        var jwt = section.Get<JwtOptions>() ?? throw new InvalidOperationException("Секція Jwt відсутня в конфігурації");
        if (Encoding.UTF8.GetByteCount(jwt.Key) < 32)
            throw new InvalidOperationException("Jwt:Key має бути не коротшим за 32 байти");

        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(o => o.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                ValidIssuer = jwt.Issuer,
                ValidAudience = jwt.Issuer,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwt.Key)),
                ClockSkew = TimeSpan.FromMinutes(1)
            });
        services.AddAuthorization();
        return services;
    }

    /// <summary>Swagger з кнопкою Authorize та XML-коментарями.</summary>
    public static IServiceCollection AddSwaggerWithJwt(this IServiceCollection services)
    {
        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen(c =>
        {
            c.SwaggerDoc("v1", new OpenApiInfo
            {
                Title = "AutoMarket API",
                Version = "v1",
                Description = "Дошка оголошень б/у техніки та авто. Гість переглядає каталог, користувач публікує оголошення, адміністратор модерує."
            });
            c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
            {
                Type = SecuritySchemeType.Http,
                Scheme = "bearer",
                BearerFormat = "JWT",
                Description = "Вставте токен з /api/auth/login (без слова Bearer)"
            });
            c.AddSecurityRequirement(new OpenApiSecurityRequirement
            {
                {
                    new OpenApiSecurityScheme { Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" } },
                    Array.Empty<string>()
                }
            });
            var xml = Path.Combine(AppContext.BaseDirectory, $"{Assembly.GetExecutingAssembly().GetName().Name}.xml");
            if (File.Exists(xml)) c.IncludeXmlComments(xml);
        });
        return services;
    }

    /// <summary>Обмеження частоти запитів на /auth (захист від перебору паролів).</summary>
    public static IServiceCollection AddAuthRateLimiting(this IServiceCollection services)
    {
        services.AddRateLimiter(o =>
        {
            o.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
            o.AddPolicy(AuthRateLimitPolicy, ctx => RateLimitPartition.GetFixedWindowLimiter(
                ctx.Connection.RemoteIpAddress?.ToString() ?? "unknown",
                _ => new FixedWindowRateLimiterOptions
                {
                    PermitLimit = ctx.RequestServices.GetRequiredService<IConfiguration>().GetValue("RateLimit:AuthPerMinute", 20),
                    Window = TimeSpan.FromMinutes(1)
                }));
        });
        return services;
    }
}

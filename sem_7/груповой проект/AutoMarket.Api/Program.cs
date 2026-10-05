using System.Text.Json.Serialization;
using AutoMarket.Api.Data;
using AutoMarket.Api.Extensions;
using AutoMarket.Api.Middleware;
using AutoMarket.Api.Services;

var builder = WebApplication.CreateBuilder(args);

// ---------- сервіси ----------
builder.Services.AddApplicationServices();
builder.Services.AddJwtAuth(builder.Configuration);
builder.Services.AddSwaggerWithJwt();
builder.Services.AddAuthRateLimiting();
builder.Services.AddHealthChecks().AddDbContextCheck<AppDbContext>("database");
builder.Services.AddCors(o => o.AddPolicy(ServiceCollectionExtensions.CorsPolicy,
    p => p.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod()));
builder.Services.AddControllers()
    .AddJsonOptions(o => o.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()));

var app = builder.Build();

// ---------- БД: міграції + сид-дані ----------
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    var hasher = scope.ServiceProvider.GetRequiredService<IPasswordHasher>();
    await DbSeeder.SeedAsync(db, hasher, withDemoData: app.Configuration.GetValue("Seed:DemoData", true));
}

// ---------- конвеєр обробки запитів (порядок важливий) ----------
app.UseMiddleware<RequestLoggingMiddleware>();
app.UseMiddleware<ExceptionHandlingMiddleware>();
// фото демо-оголошень: wwwroot/images/listings → /images/listings/*.jpg
app.UseStaticFiles();
app.UseSwagger();
app.UseSwaggerUI(c => c.DocumentTitle = "AutoMarket API");
app.UseCors(ServiceCollectionExtensions.CorsPolicy);
app.UseRateLimiter();
app.UseAuthentication();
app.UseMiddleware<BlockedUserMiddleware>();
app.UseAuthorization();

app.MapControllers();
app.MapHealthChecks("/health");
app.MapGet("/", () => Results.Redirect("/swagger")).ExcludeFromDescription();

app.Run();

/// <summary>Потрібно для інтеграційних тестів (WebApplicationFactory).</summary>
public partial class Program;

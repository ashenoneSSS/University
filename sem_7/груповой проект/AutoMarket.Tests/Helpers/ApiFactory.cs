using System.Net.Http.Headers;
using System.Net.Http.Json;
using AutoMarket.Api.Data;
using AutoMarket.Api.Dtos;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Data.Sqlite;

namespace AutoMarket.Tests.Helpers;

/// <summary>Піднімає справжній API в пам'яті з окремою тимчасовою БД.</summary>
public class ApiFactory : WebApplicationFactory<Program>
{
    private readonly string _dbPath = Path.Combine(Path.GetTempPath(), $"automarket_test_{Guid.NewGuid():N}.db");

    protected override void ConfigureWebHost(Microsoft.AspNetCore.Hosting.IWebHostBuilder builder)
    {
        builder.UseSetting("ConnectionStrings:Default", $"Data Source={_dbPath}");
        builder.UseSetting("Seed:DemoData", "false");
        builder.UseSetting("RateLimit:AuthPerMinute", "10000");
    }

    /// <summary>Клієнт, авторизований як адміністратор із сид-даних.</summary>
    public async Task<HttpClient> AdminClientAsync() =>
        await LoginAsync(DbSeeder.AdminEmail, DbSeeder.AdminPassword);

    /// <summary>Реєструє нового користувача та повертає авторизований клієнт.</summary>
    public async Task<(HttpClient Client, AuthResponse Auth)> NewUserAsync()
    {
        var client = CreateClient();
        var res = await client.PostAsJsonAsync("/api/auth/register",
            new RegisterDto { Email = $"u{Guid.NewGuid():N}@test.com", Password = "123456", Name = "Test User" });
        res.EnsureSuccessStatusCode();
        var auth = (await res.Content.ReadFromJsonAsync<AuthResponse>(Json.Options))!;
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", auth.Token);
        return (client, auth);
    }

    public async Task<HttpClient> LoginAsync(string email, string password)
    {
        var client = CreateClient();
        var res = await client.PostAsJsonAsync("/api/auth/login", new LoginDto { Email = email, Password = password });
        res.EnsureSuccessStatusCode();
        var auth = (await res.Content.ReadFromJsonAsync<AuthResponse>(Json.Options))!;
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", auth.Token);
        return client;
    }

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);
        SqliteConnection.ClearAllPools();
        try { File.Delete(_dbPath); } catch (IOException) { /* не критично */ }
    }
}

public static class Json
{
    public static readonly System.Text.Json.JsonSerializerOptions Options = new(System.Text.Json.JsonSerializerDefaults.Web)
    {
        Converters = { new System.Text.Json.Serialization.JsonStringEnumConverter() }
    };
}

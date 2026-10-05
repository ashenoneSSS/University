using System.Net;
using System.Net.Http.Json;
using AutoMarket.Api.Common;
using AutoMarket.Api.Dtos;
using AutoMarket.Tests.Helpers;
using FluentAssertions;

namespace AutoMarket.Tests.Integration;

/// <summary>Наскрізні тести через HTTP: маршрути, авторизація, middleware, валідація.</summary>
public class ApiTests(ApiFactory factory) : IClassFixture<ApiFactory>
{
    private static ListingCreateDto NewListing(string title = "Ноутбук Lenovo ThinkPad") => new()
    {
        Title = title, Description = "Робочий, батарея тримає 4 години", Price = 7500, City = "Київ", CategoryId = 4,
        PhotoUrls = new() { "https://ex.com/lenovo.jpg" }
    };

    [Fact]
    public async Task Health_ReturnsHealthy()
    {
        var res = await factory.CreateClient().GetAsync("/health");
        res.StatusCode.Should().Be(HttpStatusCode.OK);
        (await res.Content.ReadAsStringAsync()).Should().Be("Healthy");
    }

    [Fact]
    public async Task Swagger_IsAvailable()
    {
        var res = await factory.CreateClient().GetAsync("/swagger/v1/swagger.json");
        res.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task Guest_CanBrowseCatalogAndCategories()
    {
        var client = factory.CreateClient();
        (await client.GetAsync("/api/listings")).StatusCode.Should().Be(HttpStatusCode.OK);
        var cats = await client.GetFromJsonAsync<List<CategoryDto>>("/api/categories", Json.Options);
        cats.Should().HaveCountGreaterThanOrEqualTo(5);
    }

    [Theory]
    [InlineData("GET", "/api/listings/my")]
    [InlineData("POST", "/api/listings")]
    [InlineData("GET", "/api/profile")]
    [InlineData("GET", "/api/favorites")]
    [InlineData("GET", "/api/admin/listings")]
    [InlineData("GET", "/api/admin/users")]
    [InlineData("GET", "/api/admin/stats")]
    public async Task ProtectedEndpoints_WithoutToken_Return401(string method, string url)
    {
        var res = await factory.CreateClient().SendAsync(new HttpRequestMessage(new HttpMethod(method), url));
        res.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Theory]
    [InlineData("/api/admin/listings")]
    [InlineData("/api/admin/users")]
    [InlineData("/api/admin/stats")]
    public async Task AdminEndpoints_AsUser_Return403(string url)
    {
        var (client, _) = await factory.NewUserAsync();
        (await client.GetAsync(url)).StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task InvalidToken_Returns401()
    {
        var client = factory.CreateClient();
        client.DefaultRequestHeaders.Authorization = new("Bearer", "garbage.token.value");
        (await client.GetAsync("/api/profile")).StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Register_InvalidData_Returns400()
    {
        var res = await factory.CreateClient().PostAsJsonAsync("/api/auth/register",
            new RegisterDto { Email = "not-an-email", Password = "1", Name = "" });
        res.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Login_WrongPassword_Returns401WithProblemDetails()
    {
        var res = await factory.CreateClient().PostAsJsonAsync("/api/auth/login",
            new LoginDto { Email = "admin@automarket.local", Password = "wrong" });
        res.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        res.Content.Headers.ContentType!.MediaType.Should().Be("application/problem+json");
    }

    [Fact]
    public async Task CreateListing_InvalidBody_Returns400()
    {
        var (client, _) = await factory.NewUserAsync();
        var res = await client.PostAsJsonAsync("/api/listings", new ListingCreateDto { Title = "x", Description = "short", Price = -5, CategoryId = 0 });
        res.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Search_MinGreaterThanMax_Returns400()
    {
        var res = await factory.CreateClient().GetAsync("/api/listings?minPrice=100&maxPrice=1");
        res.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task UnknownListing_Returns404()
    {
        (await factory.CreateClient().GetAsync("/api/listings/999999")).StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task DemoPhotos_AreServedAsStaticFiles()
    {
        var res = await factory.CreateClient().GetAsync("/images/listings/camry.jpg");
        res.StatusCode.Should().Be(HttpStatusCode.OK);
        res.Content.Headers.ContentType!.MediaType.Should().Be("image/jpeg");
    }

    [Fact]
    public async Task Response_HasRequestIdHeader()
    {
        var res = await factory.CreateClient().GetAsync("/api/categories");
        res.Headers.Contains("X-Request-Id").Should().BeTrue();
    }

    [Fact]
    public async Task FullFlow_Create_Moderate_Find_Favorite()
    {
        var (seller, _) = await factory.NewUserAsync();
        var (buyer, _) = await factory.NewUserAsync();
        var admin = await factory.AdminClientAsync();
        var title = $"Unique {Guid.NewGuid():N}";

        // 1. продавець створює — Pending, у каталозі немає
        var createRes = await seller.PostAsJsonAsync("/api/listings", NewListing(title));
        createRes.StatusCode.Should().Be(HttpStatusCode.Created);
        var created = (await createRes.Content.ReadFromJsonAsync<ListingDto>(Json.Options))!;
        created.Status.Should().Be("Pending");
        (await Search(title)).Items.Should().BeEmpty();

        // 2. адмін бачить у черзі та схвалює
        var queue = await admin.GetFromJsonAsync<PagedResult<ListingDto>>("/api/admin/listings?pageSize=100", Json.Options);
        queue!.Items.Should().Contain(l => l.Id == created.Id);
        (await admin.PostAsync($"/api/admin/listings/{created.Id}/approve", null)).StatusCode.Should().Be(HttpStatusCode.OK);

        // 3. гість знаходить
        (await Search(title)).Items.Should().ContainSingle(l => l.Id == created.Id);

        // 4. покупець додає в обране
        (await buyer.PostAsync($"/api/favorites/{created.Id}", null)).StatusCode.Should().Be(HttpStatusCode.NoContent);
        (await buyer.GetFromJsonAsync<List<ListingDto>>("/api/favorites", Json.Options)).Should().Contain(l => l.Id == created.Id);

        // 5. покупець не може редагувати чуже
        (await buyer.PutAsJsonAsync($"/api/listings/{created.Id}", NewListing())).StatusCode.Should().Be(HttpStatusCode.Forbidden);

        // 6. продавець редагує — знову на модерацію і зникає з каталогу
        var upd = await seller.PutAsJsonAsync($"/api/listings/{created.Id}", NewListing(title + " v2"));
        (await upd.Content.ReadFromJsonAsync<ListingDto>(Json.Options))!.Status.Should().Be("Pending");
        (await Search(title)).Items.Should().BeEmpty();

        // 7. адмін відхиляє з причиною — продавець її бачить
        (await admin.PostAsJsonAsync($"/api/admin/listings/{created.Id}/reject", new RejectDto { Reason = "Додайте реальні фото" }))
            .StatusCode.Should().Be(HttpStatusCode.OK);
        var mine = await seller.GetFromJsonAsync<ListingDto>($"/api/listings/my/{created.Id}", Json.Options);
        mine!.Status.Should().Be("Rejected");
        mine.RejectReason.Should().Be("Додайте реальні фото");
    }

    [Fact]
    public async Task BlockedUser_OldTokenStopsWorking()
    {
        var (user, auth) = await factory.NewUserAsync();
        var admin = await factory.AdminClientAsync();

        (await user.GetAsync("/api/profile")).StatusCode.Should().Be(HttpStatusCode.OK);
        (await admin.PostAsync($"/api/admin/users/{auth.UserId}/block", null)).StatusCode.Should().Be(HttpStatusCode.OK);
        (await user.GetAsync("/api/profile")).StatusCode.Should().Be(HttpStatusCode.Forbidden);

        var login = await factory.CreateClient().PostAsJsonAsync("/api/auth/login", new LoginDto { Email = auth.Email, Password = "123456" });
        login.StatusCode.Should().Be(HttpStatusCode.Forbidden);

        (await admin.PostAsync($"/api/admin/users/{auth.UserId}/unblock", null)).StatusCode.Should().Be(HttpStatusCode.OK);
        (await user.GetAsync("/api/profile")).StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task DeletedUser_OldTokenReturns401()
    {
        var (user, auth) = await factory.NewUserAsync();
        var admin = await factory.AdminClientAsync();

        (await admin.DeleteAsync($"/api/admin/users/{auth.UserId}")).StatusCode.Should().Be(HttpStatusCode.NoContent);
        (await user.GetAsync("/api/profile")).StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Admin_Stats_ReturnsData()
    {
        var admin = await factory.AdminClientAsync();
        var stats = await admin.GetFromJsonAsync<StatsDto>("/api/admin/stats", Json.Options);
        stats!.TotalUsers.Should().BeGreaterThan(0);
        stats.ListingsByStatus.Keys.Should().Contain(new[] { "Pending", "Active", "Rejected", "Deleted" });
    }

    [Fact]
    public async Task Profile_UpdateAndChangePassword()
    {
        var (user, auth) = await factory.NewUserAsync();

        var upd = await user.PutAsJsonAsync("/api/profile", new UpdateProfileDto { Name = "Оновлене Ім'я", Phone = "+380501112233" });
        (await upd.Content.ReadFromJsonAsync<UserDto>(Json.Options))!.Name.Should().Be("Оновлене Ім'я");

        (await user.PostAsJsonAsync("/api/profile/change-password", new ChangePasswordDto { CurrentPassword = "123456", NewPassword = "654321" }))
            .StatusCode.Should().Be(HttpStatusCode.NoContent);
        await factory.LoginAsync(auth.Email, "654321"); // кидає, якщо не вдалося
    }

    private Task<PagedResult<ListingDto>> Search(string q) =>
        factory.CreateClient().GetFromJsonAsync<PagedResult<ListingDto>>($"/api/listings?q={Uri.EscapeDataString(q)}", Json.Options)!;
}

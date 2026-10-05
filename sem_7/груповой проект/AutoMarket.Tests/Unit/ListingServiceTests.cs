using AutoMarket.Api.Common;
using AutoMarket.Api.Dtos;
using AutoMarket.Api.Exceptions;
using AutoMarket.Api.Models;
using AutoMarket.Api.Services;
using AutoMarket.Tests.Helpers;
using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;

namespace AutoMarket.Tests.Unit;

public class ListingServiceTests : IDisposable
{
    private readonly TestDb _t = new();
    private readonly ListingService _sut;

    public ListingServiceTests() => _sut = new ListingService(_t.Db, NullLogger<ListingService>.Instance);

    public void Dispose() => _t.Dispose();

    private static ListingCreateDto Dto(string title = "BMW X5 2015", decimal price = 20000, int categoryId = 1, List<string>? photos = null) =>
        new() { Title = title, Description = "Гарний стан, без ДТП", Price = price, City = "Київ", CategoryId = categoryId, PhotoUrls = photos };

    // ---------- створення ----------

    [Fact]
    public async Task Create_SetsPendingStatusAndOwner()
    {
        var user = await _t.AddUserAsync();
        var res = await _sut.CreateAsync(user.Id, Dto(photos: new() { "https://ex.com/1.jpg" }));

        res.Status.Should().Be("Pending");
        res.AuthorId.Should().Be(user.Id);
        res.Category.Should().Be("Авто");
        res.Photos.Should().ContainSingle();
    }

    [Fact]
    public async Task Create_UnknownCategory_Throws400()
    {
        var user = await _t.AddUserAsync();
        var act = () => _sut.CreateAsync(user.Id, Dto(categoryId: 999));
        await act.Should().ThrowAsync<BadRequestException>();
    }

    [Theory]
    [InlineData("not a url")]
    [InlineData("ftp://ex.com/a.jpg")]
    [InlineData("/relative/path.jpg")]
    public async Task Create_InvalidPhotoUrl_Throws400(string url)
    {
        var user = await _t.AddUserAsync();
        var act = () => _sut.CreateAsync(user.Id, Dto(photos: new() { url }));
        await act.Should().ThrowAsync<BadRequestException>();
    }

    [Fact]
    public async Task Create_LocalDemoPhoto_IsAllowed()
    {
        var user = await _t.AddUserAsync();
        var res = await _sut.CreateAsync(user.Id, Dto(photos: new() { "/images/listings/camry.jpg" }));
        res.Photos.Should().ContainSingle("/images/listings/camry.jpg");
    }

    [Fact]
    public async Task Create_PathTraversalInLocalPhoto_Throws400()
    {
        var user = await _t.AddUserAsync();
        var act = () => _sut.CreateAsync(user.Id, Dto(photos: new() { "/images/listings/../../appsettings.json" }));
        await act.Should().ThrowAsync<BadRequestException>();
    }

    [Fact]
    public async Task Create_TooManyPhotos_Throws400()
    {
        var user = await _t.AddUserAsync();
        var photos = Enumerable.Range(1, 11).Select(i => $"https://ex.com/{i}.jpg").ToList();
        var act = () => _sut.CreateAsync(user.Id, Dto(photos: photos));
        await act.Should().ThrowAsync<BadRequestException>();
    }

    [Fact]
    public async Task Create_DuplicatePhotoUrls_AreDeduplicated()
    {
        var user = await _t.AddUserAsync();
        var res = await _sut.CreateAsync(user.Id, Dto(photos: new() { "https://ex.com/1.jpg", "https://ex.com/1.jpg" }));
        res.Photos.Should().HaveCount(1);
    }

    // ---------- каталог ----------

    [Fact]
    public async Task Search_ReturnsOnlyActive()
    {
        var u = await _t.AddUserAsync();
        await _t.AddListingAsync(u.Id, "Active", status: ListingStatus.Active);
        await _t.AddListingAsync(u.Id, "Pending", status: ListingStatus.Pending);
        await _t.AddListingAsync(u.Id, "Rejected", status: ListingStatus.Rejected);
        await _t.AddListingAsync(u.Id, "Deleted", status: ListingStatus.Deleted);

        var res = await _sut.SearchAsync(new ListingQuery());

        res.Items.Should().ContainSingle().Which.Title.Should().Be("Active");
        res.TotalCount.Should().Be(1);
    }

    [Fact]
    public async Task Search_ByText_IsCaseInsensitive()
    {
        var u = await _t.AddUserAsync();
        await _t.AddListingAsync(u.Id, "BMW X5");
        await _t.AddListingAsync(u.Id, "Audi A6");

        var res = await _sut.SearchAsync(new ListingQuery { Q = "bmw" });
        res.Items.Should().ContainSingle().Which.Title.Should().Be("BMW X5");
    }

    [Theory]
    [InlineData("тойота")]
    [InlineData("ТОЙОТА")]
    [InlineData("Камрі")]
    public async Task Search_Cyrillic_IsCaseInsensitive(string q)
    {
        var u = await _t.AddUserAsync();
        await _t.AddListingAsync(u.Id, "Тойота Камрі 2012");
        await _t.AddListingAsync(u.Id, "Хонда Сівік");

        var res = await _sut.SearchAsync(new ListingQuery { Q = q });
        res.Items.Should().ContainSingle().Which.Title.Should().Be("Тойота Камрі 2012");
    }

    [Fact]
    public async Task Search_ByCyrillicCity_IgnoresCase()
    {
        var u = await _t.AddUserAsync();
        await _t.AddListingAsync(u.Id, "A", city: "Львів");
        await _t.AddListingAsync(u.Id, "B", city: "Київ");

        var res = await _sut.SearchAsync(new ListingQuery { City = "ЛЬВІВ" });
        res.Items.Should().ContainSingle().Which.Title.Should().Be("A");
    }

    [Fact]
    public async Task Search_ByPriceRangeAndCategory()
    {
        var u = await _t.AddUserAsync();
        await _t.AddListingAsync(u.Id, "Cheap car", 500, categoryId: 1);
        await _t.AddListingAsync(u.Id, "Mid car", 5000, categoryId: 1);
        await _t.AddListingAsync(u.Id, "Mid phone", 5000, categoryId: 3);
        await _t.AddListingAsync(u.Id, "Expensive car", 50000, categoryId: 1);

        var res = await _sut.SearchAsync(new ListingQuery { MinPrice = 1000, MaxPrice = 10000, CategoryId = 1 });
        res.Items.Should().ContainSingle().Which.Title.Should().Be("Mid car");
    }

    [Fact]
    public async Task Search_ByCity_IgnoresCase()
    {
        var u = await _t.AddUserAsync();
        await _t.AddListingAsync(u.Id, "In Lviv", city: "Lviv");
        await _t.AddListingAsync(u.Id, "In Kyiv", city: "Kyiv");

        var res = await _sut.SearchAsync(new ListingQuery { City = "lviv" });
        res.Items.Should().ContainSingle().Which.Title.Should().Be("In Lviv");
    }

    [Fact]
    public async Task Search_MinGreaterThanMax_Throws400()
    {
        var act = () => _sut.SearchAsync(new ListingQuery { MinPrice = 100, MaxPrice = 10 });
        await act.Should().ThrowAsync<BadRequestException>();
    }

    [Fact]
    public async Task Search_SortsByPrice()
    {
        var u = await _t.AddUserAsync();
        await _t.AddListingAsync(u.Id, "B", 300);
        await _t.AddListingAsync(u.Id, "A", 100);
        await _t.AddListingAsync(u.Id, "C", 200);

        var asc = await _sut.SearchAsync(new ListingQuery { Sort = ListingSort.PriceAsc });
        var desc = await _sut.SearchAsync(new ListingQuery { Sort = ListingSort.PriceDesc });

        asc.Items.Select(i => i.Price).Should().BeInAscendingOrder();
        desc.Items.Select(i => i.Price).Should().BeInDescendingOrder();
    }

    [Fact]
    public async Task Search_Newest_IsDefaultSort()
    {
        var u = await _t.AddUserAsync();
        await _t.AddListingAsync(u.Id, "Old", createdAt: DateTime.UtcNow.AddDays(-2));
        await _t.AddListingAsync(u.Id, "New", createdAt: DateTime.UtcNow);

        var res = await _sut.SearchAsync(new ListingQuery());
        res.Items.First().Title.Should().Be("New");
    }

    [Fact]
    public async Task Search_Paginates()
    {
        var u = await _t.AddUserAsync();
        for (var i = 0; i < 25; i++) await _t.AddListingAsync(u.Id, $"Item {i}");

        var page3 = await _sut.SearchAsync(new ListingQuery { Page = 3, PageSize = 10 });

        page3.Items.Should().HaveCount(5);
        page3.TotalCount.Should().Be(25);
        page3.TotalPages.Should().Be(3);
        page3.HasNext.Should().BeFalse();
    }

    [Fact]
    public async Task GetActive_IncrementsViews()
    {
        var u = await _t.AddUserAsync();
        var l = await _t.AddListingAsync(u.Id);

        await _sut.GetActiveAsync(l.Id);
        var second = await _sut.GetActiveAsync(l.Id);

        second.Views.Should().Be(2);
    }

    [Fact]
    public async Task GetActive_PendingListing_Throws404()
    {
        var u = await _t.AddUserAsync();
        var l = await _t.AddListingAsync(u.Id, status: ListingStatus.Pending);
        var act = () => _sut.GetActiveAsync(l.Id);
        await act.Should().ThrowAsync<NotFoundException>();
    }

    // ---------- кабінет ----------

    [Fact]
    public async Task GetByUser_ReturnsOwnNonDeletedListingsOnly()
    {
        var me = await _t.AddUserAsync("me@test.com");
        var other = await _t.AddUserAsync("other@test.com");
        await _t.AddListingAsync(me.Id, "Mine pending", status: ListingStatus.Pending);
        await _t.AddListingAsync(me.Id, "Mine rejected", status: ListingStatus.Rejected);
        await _t.AddListingAsync(me.Id, "Mine deleted", status: ListingStatus.Deleted);
        await _t.AddListingAsync(other.Id, "Not mine");

        var res = await _sut.GetByUserAsync(me.Id, null);

        res.Select(r => r.Title).Should().BeEquivalentTo("Mine pending", "Mine rejected");
    }

    [Fact]
    public async Task Update_ResetsStatusToPending_AndClearsRejectReason()
    {
        var u = await _t.AddUserAsync();
        var l = await _t.AddListingAsync(u.Id, status: ListingStatus.Rejected);
        l.RejectReason = "Погані фото";
        await _t.Db.SaveChangesAsync();

        var res = await _sut.UpdateAsync(u.Id, l.Id, Dto(title: "Нова назва"));

        res.Status.Should().Be("Pending");
        res.RejectReason.Should().BeNull();
        res.Title.Should().Be("Нова назва");
        res.UpdatedAt.Should().NotBeNull();
    }

    [Fact]
    public async Task Update_ReplacesPhotos()
    {
        var u = await _t.AddUserAsync();
        var created = await _sut.CreateAsync(u.Id, Dto(photos: new() { "https://ex.com/old.jpg" }));

        var res = await _sut.UpdateAsync(u.Id, created.Id, Dto(photos: new() { "https://ex.com/new1.jpg", "https://ex.com/new2.jpg" }));

        res.Photos.Should().BeEquivalentTo("https://ex.com/new1.jpg", "https://ex.com/new2.jpg");
        _t.Db.Photos.Count().Should().Be(2);
    }

    [Fact]
    public async Task Update_SomeoneElsesListing_Throws403()
    {
        var owner = await _t.AddUserAsync("owner@test.com");
        var hacker = await _t.AddUserAsync("hacker@test.com");
        var l = await _t.AddListingAsync(owner.Id);

        var act = () => _sut.UpdateAsync(hacker.Id, l.Id, Dto());
        await act.Should().ThrowAsync<ForbiddenException>();
    }

    [Fact]
    public async Task Update_DeletedListing_Throws404()
    {
        var u = await _t.AddUserAsync();
        var l = await _t.AddListingAsync(u.Id, status: ListingStatus.Deleted);
        var act = () => _sut.UpdateAsync(u.Id, l.Id, Dto());
        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task Delete_IsSoft()
    {
        var u = await _t.AddUserAsync();
        var l = await _t.AddListingAsync(u.Id);

        await _sut.DeleteAsync(u.Id, l.Id);

        _t.Db.Listings.Single(x => x.Id == l.Id).Status.Should().Be(ListingStatus.Deleted);
    }

    [Fact]
    public async Task Delete_SomeoneElsesListing_Throws403()
    {
        var owner = await _t.AddUserAsync("owner@test.com");
        var other = await _t.AddUserAsync("other@test.com");
        var l = await _t.AddListingAsync(owner.Id);

        var act = () => _sut.DeleteAsync(other.Id, l.Id);
        await act.Should().ThrowAsync<ForbiddenException>();
    }

    // ---------- модерація ----------

    [Fact]
    public async Task Approve_MakesListingVisibleInCatalog()
    {
        var u = await _t.AddUserAsync();
        var l = await _t.AddListingAsync(u.Id, status: ListingStatus.Pending);

        (await _sut.SearchAsync(new ListingQuery())).Items.Should().BeEmpty();
        await _sut.ApproveAsync(l.Id);
        (await _sut.SearchAsync(new ListingQuery())).Items.Should().ContainSingle();
    }

    [Fact]
    public async Task Reject_StoresReason()
    {
        var u = await _t.AddUserAsync();
        var l = await _t.AddListingAsync(u.Id, status: ListingStatus.Pending);

        var res = await _sut.RejectAsync(l.Id, "Заборонений товар");

        res.Status.Should().Be("Rejected");
        res.RejectReason.Should().Be("Заборонений товар");
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public async Task Reject_EmptyReason_Throws400(string reason)
    {
        var u = await _t.AddUserAsync();
        var l = await _t.AddListingAsync(u.Id, status: ListingStatus.Pending);
        var act = () => _sut.RejectAsync(l.Id, reason);
        await act.Should().ThrowAsync<BadRequestException>();
    }

    [Fact]
    public async Task Approve_DeletedListing_Throws400()
    {
        var u = await _t.AddUserAsync();
        var l = await _t.AddListingAsync(u.Id, status: ListingStatus.Deleted);
        var act = () => _sut.ApproveAsync(l.Id);
        await act.Should().ThrowAsync<BadRequestException>();
    }

    [Fact]
    public async Task Approve_Missing_Throws404()
    {
        var act = () => _sut.ApproveAsync(12345);
        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task ModerationQueue_IsOldestFirst()
    {
        var u = await _t.AddUserAsync();
        await _t.AddListingAsync(u.Id, "Newer", status: ListingStatus.Pending, createdAt: DateTime.UtcNow);
        await _t.AddListingAsync(u.Id, "Older", status: ListingStatus.Pending, createdAt: DateTime.UtcNow.AddHours(-5));

        var res = await _sut.GetForModerationAsync(ListingStatus.Pending, new PageRequest());
        res.Items.Select(i => i.Title).Should().ContainInOrder("Older", "Newer");
    }
}

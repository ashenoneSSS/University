using AutoMarket.Api.Dtos;
using AutoMarket.Api.Exceptions;
using AutoMarket.Api.Models;
using AutoMarket.Api.Services;
using AutoMarket.Tests.Helpers;
using FluentAssertions;

namespace AutoMarket.Tests.Unit;

public class CategoryServiceTests : IDisposable
{
    private readonly TestDb _t = new();
    private readonly CategoryService _sut;

    public CategoryServiceTests() => _sut = new CategoryService(_t.Db);

    public void Dispose() => _t.Dispose();

    [Fact]
    public async Task GetAll_ReturnsSeededCategories()
    {
        (await _sut.GetAllAsync()).Should().HaveCount(6);
    }

    [Fact]
    public async Task GetAll_CountsOnlyActiveListings()
    {
        var u = await _t.AddUserAsync();
        await _t.AddListingAsync(u.Id, categoryId: 2);
        await _t.AddListingAsync(u.Id, categoryId: 2, status: ListingStatus.Pending);

        (await _sut.GetAsync(2)).ActiveListings.Should().Be(1);
    }

    [Fact]
    public async Task Create_Duplicate_IgnoringCase_Throws409()
    {
        var act = () => _sut.CreateAsync(new CategoryCreateDto { Name = "авто" });
        await act.Should().ThrowAsync<ConflictException>();
    }

    [Fact]
    public async Task Create_Update_Delete_Roundtrip()
    {
        var c = await _sut.CreateAsync(new CategoryCreateDto { Name = "Велосипеди" });
        (await _sut.UpdateAsync(c.Id, new CategoryCreateDto { Name = "Велосипеди та самокати" })).Name.Should().Be("Велосипеди та самокати");
        await _sut.DeleteAsync(c.Id);
        var act = () => _sut.GetAsync(c.Id);
        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task Delete_WithListings_Throws409()
    {
        var u = await _t.AddUserAsync();
        await _t.AddListingAsync(u.Id, categoryId: 1);
        var act = () => _sut.DeleteAsync(1);
        await act.Should().ThrowAsync<ConflictException>();
    }
}

public class FavoriteServiceTests : IDisposable
{
    private readonly TestDb _t = new();
    private readonly FavoriteService _sut;

    public FavoriteServiceTests() => _sut = new FavoriteService(_t.Db);

    public void Dispose() => _t.Dispose();

    [Fact]
    public async Task Add_IsIdempotent()
    {
        var seller = await _t.AddUserAsync("seller@test.com");
        var buyer = await _t.AddUserAsync("buyer@test.com");
        var l = await _t.AddListingAsync(seller.Id);

        await _sut.AddAsync(buyer.Id, l.Id);
        await _sut.AddAsync(buyer.Id, l.Id);

        (await _sut.GetAsync(buyer.Id)).Should().ContainSingle();
    }

    [Fact]
    public async Task Add_NotActiveListing_Throws404()
    {
        var seller = await _t.AddUserAsync("seller@test.com");
        var buyer = await _t.AddUserAsync("buyer@test.com");
        var l = await _t.AddListingAsync(seller.Id, status: ListingStatus.Pending);

        var act = () => _sut.AddAsync(buyer.Id, l.Id);
        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task Add_OwnListing_Throws400()
    {
        var seller = await _t.AddUserAsync("seller@test.com");
        var l = await _t.AddListingAsync(seller.Id);
        var act = () => _sut.AddAsync(seller.Id, l.Id);
        await act.Should().ThrowAsync<BadRequestException>();
    }

    [Fact]
    public async Task Get_HidesListingsThatBecameInactive()
    {
        var seller = await _t.AddUserAsync("seller@test.com");
        var buyer = await _t.AddUserAsync("buyer@test.com");
        var l = await _t.AddListingAsync(seller.Id);
        await _sut.AddAsync(buyer.Id, l.Id);

        l.Status = ListingStatus.Deleted;
        await _t.Db.SaveChangesAsync();

        (await _sut.GetAsync(buyer.Id)).Should().BeEmpty();
    }

    [Fact]
    public async Task Remove_NotInFavorites_Throws404()
    {
        var u = await _t.AddUserAsync();
        var act = () => _sut.RemoveAsync(u.Id, 1);
        await act.Should().ThrowAsync<NotFoundException>();
    }
}

using AutoMarket.Api.Models;
using AutoMarket.Api.Services;
using AutoMarket.Tests.Helpers;
using FluentAssertions;

namespace AutoMarket.Tests.Unit;

public class StatsServiceTests : IDisposable
{
    private readonly TestDb _t = new();

    public void Dispose() => _t.Dispose();

    [Fact]
    public async Task Get_AggregatesCorrectly()
    {
        var u = await _t.AddUserAsync();
        await _t.AddUserAsync("blocked@test.com", blocked: true);
        await _t.AddListingAsync(u.Id, price: 100);
        await _t.AddListingAsync(u.Id, price: 300);
        await _t.AddListingAsync(u.Id, status: ListingStatus.Pending);

        var sut = new StatsService(_t.Db, new CategoryService(_t.Db));
        var s = await sut.GetAsync();

        s.TotalUsers.Should().Be(2);
        s.BlockedUsers.Should().Be(1);
        s.TotalListings.Should().Be(3);
        s.ListingsByStatus["Active"].Should().Be(2);
        s.ListingsByStatus["Pending"].Should().Be(1);
        s.ListingsByStatus["Rejected"].Should().Be(0);
        s.AverageActivePrice.Should().Be(200);
        s.NewListingsLast7Days.Should().Be(3);
    }

    [Fact]
    public async Task Get_EmptyDatabase_DoesNotThrow()
    {
        var sut = new StatsService(_t.Db, new CategoryService(_t.Db));
        var s = await sut.GetAsync();
        s.AverageActivePrice.Should().Be(0);
        s.TotalListings.Should().Be(0);
    }
}

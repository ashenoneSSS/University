using AutoMarket.Api.Common;
using FluentAssertions;

namespace AutoMarket.Tests.Unit;

public class PagingTests
{
    [Theory]
    [InlineData(0, 1)]
    [InlineData(-5, 1)]
    [InlineData(3, 3)]
    public void Page_IsAtLeastOne(int input, int expected) =>
        new PageRequest { Page = input }.Page.Should().Be(expected);

    [Theory]
    [InlineData(0, 1)]
    [InlineData(500, PageRequest.MaxPageSize)]
    [InlineData(10, 10)]
    public void PageSize_IsClamped(int input, int expected) =>
        new PageRequest { PageSize = input }.PageSize.Should().Be(expected);

    [Fact]
    public void Skip_IsCalculatedFromPage() =>
        new PageRequest { Page = 3, PageSize = 10 }.Skip.Should().Be(20);

    [Fact]
    public void PagedResult_ComputesNavigation()
    {
        var r = new PagedResult<int> { Items = new() { 1 }, Page = 2, PageSize = 10, TotalCount = 25 };
        r.TotalPages.Should().Be(3);
        r.HasNext.Should().BeTrue();
        r.HasPrevious.Should().BeTrue();
    }
}

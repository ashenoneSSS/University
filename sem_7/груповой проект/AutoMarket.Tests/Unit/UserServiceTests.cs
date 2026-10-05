using AutoMarket.Api.Common;
using AutoMarket.Api.Dtos;
using AutoMarket.Api.Exceptions;
using AutoMarket.Api.Models;
using AutoMarket.Api.Services;
using AutoMarket.Tests.Helpers;
using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;

namespace AutoMarket.Tests.Unit;

public class UserServiceTests : IDisposable
{
    private readonly TestDb _t = new();
    private readonly UserService _sut;

    public UserServiceTests() => _sut = new UserService(_t.Db, _t.Hasher, NullLogger<UserService>.Instance);

    public void Dispose() => _t.Dispose();

    [Fact]
    public async Task UpdateProfile_ChangesNameAndPhone()
    {
        var u = await _t.AddUserAsync();
        var res = await _sut.UpdateProfileAsync(u.Id, new UpdateProfileDto { Name = " Новий ", Phone = "+380991112233" });
        res.Name.Should().Be("Новий");
        res.Phone.Should().Be("+380991112233");
    }

    [Fact]
    public async Task ChangePassword_WithCorrectCurrent_Works()
    {
        var u = await _t.AddUserAsync(password: "old-pass");
        await _sut.ChangePasswordAsync(u.Id, new ChangePasswordDto { CurrentPassword = "old-pass", NewPassword = "new-pass" });
        _t.Hasher.Verify("new-pass", _t.Db.Users.Single(x => x.Id == u.Id).PasswordHash).Should().BeTrue();
    }

    [Fact]
    public async Task ChangePassword_WrongCurrent_Throws400()
    {
        var u = await _t.AddUserAsync(password: "old-pass");
        var act = () => _sut.ChangePasswordAsync(u.Id, new ChangePasswordDto { CurrentPassword = "nope", NewPassword = "new-pass" });
        await act.Should().ThrowAsync<BadRequestException>();
    }

    [Fact]
    public async Task ChangePassword_SameAsOld_Throws400()
    {
        var u = await _t.AddUserAsync(password: "old-pass");
        var act = () => _sut.ChangePasswordAsync(u.Id, new ChangePasswordDto { CurrentPassword = "old-pass", NewPassword = "old-pass" });
        await act.Should().ThrowAsync<BadRequestException>();
    }

    [Fact]
    public async Task Block_SetsFlag()
    {
        var admin = await _t.AddUserAsync("admin@test.com", UserRole.Admin);
        var u = await _t.AddUserAsync();
        var res = await _sut.SetBlockedAsync(admin.Id, u.Id, true);
        res.IsBlocked.Should().BeTrue();
    }

    [Fact]
    public async Task Block_Self_Throws400()
    {
        var admin = await _t.AddUserAsync("admin@test.com", UserRole.Admin);
        var act = () => _sut.SetBlockedAsync(admin.Id, admin.Id, true);
        await act.Should().ThrowAsync<BadRequestException>();
    }

    [Fact]
    public async Task SetRole_DemoteSelf_Throws400()
    {
        var admin = await _t.AddUserAsync("admin@test.com", UserRole.Admin);
        var act = () => _sut.SetRoleAsync(admin.Id, admin.Id, UserRole.User);
        await act.Should().ThrowAsync<BadRequestException>();
    }

    [Fact]
    public async Task SetRole_PromoteUser_Works()
    {
        var admin = await _t.AddUserAsync("admin@test.com", UserRole.Admin);
        var u = await _t.AddUserAsync();
        (await _sut.SetRoleAsync(admin.Id, u.Id, UserRole.Admin)).Role.Should().Be("Admin");
    }

    [Fact]
    public async Task SetRole_DemoteOtherAdmin_WhenTwoAdmins_Works()
    {
        var a1 = await _t.AddUserAsync("a1@test.com", UserRole.Admin);
        var a2 = await _t.AddUserAsync("a2@test.com", UserRole.Admin);
        (await _sut.SetRoleAsync(a1.Id, a2.Id, UserRole.User)).Role.Should().Be("User");
    }

    [Fact]
    public async Task Delete_RemovesUserAndListings()
    {
        var admin = await _t.AddUserAsync("admin@test.com", UserRole.Admin);
        var u = await _t.AddUserAsync();
        await _t.AddListingAsync(u.Id);

        await _sut.DeleteAsync(admin.Id, u.Id);

        _t.Db.Users.Any(x => x.Id == u.Id).Should().BeFalse();
        _t.Db.Listings.Any(x => x.UserId == u.Id).Should().BeFalse();
    }

    [Fact]
    public async Task Delete_Self_Throws400()
    {
        var admin = await _t.AddUserAsync("admin@test.com", UserRole.Admin);
        var act = () => _sut.DeleteAsync(admin.Id, admin.Id);
        await act.Should().ThrowAsync<BadRequestException>();
    }

    [Fact]
    public async Task GetAll_SearchesByEmailOrName_AndCountsListings()
    {
        var u = await _t.AddUserAsync("ivan@test.com");
        await _t.AddUserAsync("petro@test.com");
        await _t.AddListingAsync(u.Id);
        await _t.AddListingAsync(u.Id, status: ListingStatus.Deleted);

        var res = await _sut.GetAllAsync("IVAN", new PageRequest());

        res.Items.Should().ContainSingle().Which.ListingsCount.Should().Be(1);
    }

    [Fact]
    public async Task Get_Missing_Throws404()
    {
        var act = () => _sut.GetAsync(999);
        await act.Should().ThrowAsync<NotFoundException>();
    }
}

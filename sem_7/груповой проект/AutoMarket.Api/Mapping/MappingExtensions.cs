using AutoMarket.Api.Dtos;
using AutoMarket.Api.Models;

namespace AutoMarket.Api.Mapping;

/// <summary>Перетворення сутностей у DTO.</summary>
public static class MappingExtensions
{
    public static ListingDto ToDto(this Listing l) => new(
        l.Id, l.Title, l.Description, l.Price, l.City,
        l.Status.ToString(), l.RejectReason,
        l.CategoryId, l.Category?.Name ?? "",
        l.UserId, l.User?.Name ?? "", l.User?.Phone,
        l.Views, l.CreatedAt, l.UpdatedAt,
        l.Photos.Select(p => p.Url).ToList());

    public static UserDto ToDto(this User u, int listingsCount = 0) =>
        new(u.Id, u.Email, u.Name, u.Phone, u.Role.ToString(), u.IsBlocked, u.CreatedAt, listingsCount);
}

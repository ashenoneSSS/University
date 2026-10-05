using System.Security.Claims;
using AutoMarket.Api.Exceptions;

namespace AutoMarket.Api.Extensions;

public static class ClaimsPrincipalExtensions
{
    /// <summary>Id поточного користувача з JWT.</summary>
    public static int GetUserId(this ClaimsPrincipal user) =>
        int.TryParse(user.FindFirstValue(ClaimTypes.NameIdentifier), out var id)
            ? id
            : throw new UnauthorizedException("Невалідний токен");
}

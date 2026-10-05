using AutoMarket.Api.Data;
using AutoMarket.Api.Exceptions;
using AutoMarket.Api.Extensions;
using Microsoft.EntityFrameworkCore;

namespace AutoMarket.Api.Middleware;

/// <summary>
/// JWT діє 7 днів, тому після блокування чи видалення користувача старий токен ще валідний.
/// Цей middleware перевіряє БД на кожному авторизованому запиті та відсікає такі токени.
/// </summary>
public class BlockedUserMiddleware(RequestDelegate next)
{
    public async Task InvokeAsync(HttpContext context, AppDbContext db)
    {
        if (context.User.Identity?.IsAuthenticated == true)
        {
            var userId = context.User.GetUserId();
            var state = await db.Users.AsNoTracking().Where(u => u.Id == userId)
                .Select(u => new { u.IsBlocked })
                .FirstOrDefaultAsync();

            if (state == null)
                throw new UnauthorizedException("Користувача більше не існує");
            if (state.IsBlocked)
                throw new ForbiddenException("Користувача заблоковано адміністратором");
        }
        await next(context);
    }
}

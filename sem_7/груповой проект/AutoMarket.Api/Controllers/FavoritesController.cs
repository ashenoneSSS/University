using AutoMarket.Api.Dtos;
using AutoMarket.Api.Extensions;
using AutoMarket.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AutoMarket.Api.Controllers;

/// <summary>Обрані оголошення поточного користувача.</summary>
[ApiController]
[Route("api/favorites")]
[Authorize]
[Produces("application/json")]
public class FavoritesController(IFavoriteService favorites) : ControllerBase
{
    /// <summary>Список обраних (лише активні оголошення).</summary>
    [HttpGet]
    public Task<List<ListingDto>> Get() => favorites.GetAsync(User.GetUserId());

    /// <summary>Додати оголошення в обране. Повторне додавання не є помилкою.</summary>
    [HttpPost("{listingId:int}")]
    [ProducesResponseType(204)]
    [ProducesResponseType(typeof(ProblemDetails), 400)]
    [ProducesResponseType(typeof(ProblemDetails), 404)]
    public async Task<IActionResult> Add(int listingId)
    {
        await favorites.AddAsync(User.GetUserId(), listingId);
        return NoContent();
    }

    /// <summary>Прибрати з обраного.</summary>
    [HttpDelete("{listingId:int}")]
    [ProducesResponseType(204)]
    [ProducesResponseType(typeof(ProblemDetails), 404)]
    public async Task<IActionResult> Remove(int listingId)
    {
        await favorites.RemoveAsync(User.GetUserId(), listingId);
        return NoContent();
    }
}

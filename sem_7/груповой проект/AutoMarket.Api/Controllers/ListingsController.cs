using AutoMarket.Api.Common;
using AutoMarket.Api.Dtos;
using AutoMarket.Api.Extensions;
using AutoMarket.Api.Models;
using AutoMarket.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AutoMarket.Api.Controllers;

/// <summary>Каталог оголошень (для всіх) та керування власними оголошеннями.</summary>
[ApiController]
[Route("api/listings")]
[Produces("application/json")]
public class ListingsController(IListingService listings) : ControllerBase
{
    /// <summary>Пошук серед активних оголошень з фільтрами, сортуванням і пагінацією. Доступно гостям.</summary>
    /// <remarks>Приклад: <c>GET /api/listings?q=bmw&amp;categoryId=1&amp;maxPrice=20000&amp;sort=PriceAsc&amp;page=1&amp;pageSize=20</c></remarks>
    [HttpGet]
    [ProducesResponseType(typeof(PagedResult<ListingDto>), 200)]
    [ProducesResponseType(typeof(ProblemDetails), 400)]
    public Task<PagedResult<ListingDto>> Search([FromQuery] ListingQuery query) => listings.SearchAsync(query);

    /// <summary>Перегляд активного оголошення. Збільшує лічильник переглядів.</summary>
    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(ListingDto), 200)]
    [ProducesResponseType(typeof(ProblemDetails), 404)]
    public Task<ListingDto> Get(int id) => listings.GetActiveAsync(id);

    /// <summary>Мої оголошення з усіма статусами (крім видалених). Можна відфільтрувати за статусом.</summary>
    [HttpGet("my"), Authorize]
    [ProducesResponseType(typeof(List<ListingDto>), 200)]
    public Task<List<ListingDto>> My([FromQuery] ListingStatus? status) => listings.GetByUserAsync(User.GetUserId(), status);

    /// <summary>Одне моє оголошення (будь-який статус) — наприклад, щоб побачити причину відхилення.</summary>
    [HttpGet("my/{id:int}"), Authorize]
    [ProducesResponseType(typeof(ListingDto), 200)]
    [ProducesResponseType(typeof(ProblemDetails), 403)]
    [ProducesResponseType(typeof(ProblemDetails), 404)]
    public Task<ListingDto> MyOne(int id) => listings.GetOwnAsync(User.GetUserId(), id);

    /// <summary>Створити оголошення. Потрапляє на модерацію зі статусом Pending.</summary>
    [HttpPost, Authorize]
    [ProducesResponseType(typeof(ListingDto), 201)]
    [ProducesResponseType(typeof(ProblemDetails), 400)]
    public async Task<ActionResult<ListingDto>> Create(ListingCreateDto dto)
    {
        var created = await listings.CreateAsync(User.GetUserId(), dto);
        return CreatedAtAction(nameof(MyOne), new { id = created.Id }, created);
    }

    /// <summary>Редагувати своє оголошення. Після змін воно знову йде на модерацію.</summary>
    [HttpPut("{id:int}"), Authorize]
    [ProducesResponseType(typeof(ListingDto), 200)]
    [ProducesResponseType(typeof(ProblemDetails), 400)]
    [ProducesResponseType(typeof(ProblemDetails), 403)]
    [ProducesResponseType(typeof(ProblemDetails), 404)]
    public Task<ListingDto> Update(int id, ListingCreateDto dto) => listings.UpdateAsync(User.GetUserId(), id, dto);

    /// <summary>Видалити своє оголошення (м'яке видалення, статус Deleted).</summary>
    [HttpDelete("{id:int}"), Authorize]
    [ProducesResponseType(204)]
    [ProducesResponseType(typeof(ProblemDetails), 403)]
    [ProducesResponseType(typeof(ProblemDetails), 404)]
    public async Task<IActionResult> Delete(int id)
    {
        await listings.DeleteAsync(User.GetUserId(), id);
        return NoContent();
    }
}

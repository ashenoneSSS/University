using AutoMarket.Api.Common;
using AutoMarket.Api.Dtos;
using AutoMarket.Api.Extensions;
using AutoMarket.Api.Models;
using AutoMarket.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AutoMarket.Api.Controllers.Admin;

/// <summary>Керування користувачами (Admin).</summary>
[ApiController]
[Route("api/admin/users")]
[Authorize(Roles = "Admin")]
[Produces("application/json")]
public class AdminUsersController(IUserService users, IListingService listings) : ControllerBase
{
    /// <summary>Список користувачів з пошуком за email/ім'ям.</summary>
    [HttpGet]
    public Task<PagedResult<UserDto>> GetAll([FromQuery] string? search, [FromQuery] PageRequest? page = null) =>
        users.GetAllAsync(search, page ?? new PageRequest());

    /// <summary>Користувач за id.</summary>
    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(UserDto), 200)]
    [ProducesResponseType(typeof(ProblemDetails), 404)]
    public Task<UserDto> Get(int id) => users.GetAsync(id);

    /// <summary>Усі оголошення користувача.</summary>
    [HttpGet("{id:int}/listings")]
    public Task<List<ListingDto>> Listings(int id, [FromQuery] ListingStatus? status) => listings.GetByUserAsync(id, status);

    /// <summary>Заблокувати користувача (не може увійти, старі токени перестають діяти).</summary>
    [HttpPost("{id:int}/block")]
    [ProducesResponseType(typeof(UserDto), 200)]
    [ProducesResponseType(typeof(ProblemDetails), 400)]
    public Task<UserDto> Block(int id) => users.SetBlockedAsync(User.GetUserId(), id, true);

    /// <summary>Розблокувати користувача.</summary>
    [HttpPost("{id:int}/unblock")]
    [ProducesResponseType(typeof(UserDto), 200)]
    public Task<UserDto> Unblock(int id) => users.SetBlockedAsync(User.GetUserId(), id, false);

    /// <summary>Змінити роль (User / Admin).</summary>
    [HttpPut("{id:int}/role")]
    [ProducesResponseType(typeof(UserDto), 200)]
    [ProducesResponseType(typeof(ProblemDetails), 400)]
    public Task<UserDto> SetRole(int id, SetRoleDto dto) => users.SetRoleAsync(User.GetUserId(), id, dto.Role);

    /// <summary>Видалити користувача разом з його оголошеннями.</summary>
    [HttpDelete("{id:int}")]
    [ProducesResponseType(204)]
    [ProducesResponseType(typeof(ProblemDetails), 400)]
    [ProducesResponseType(typeof(ProblemDetails), 404)]
    public async Task<IActionResult> Delete(int id)
    {
        await users.DeleteAsync(User.GetUserId(), id);
        return NoContent();
    }
}

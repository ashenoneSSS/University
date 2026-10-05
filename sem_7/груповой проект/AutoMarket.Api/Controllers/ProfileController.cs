using AutoMarket.Api.Dtos;
using AutoMarket.Api.Extensions;
using AutoMarket.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AutoMarket.Api.Controllers;

/// <summary>Особистий кабінет: профіль і зміна пароля.</summary>
[ApiController]
[Route("api/profile")]
[Authorize]
[Produces("application/json")]
public class ProfileController(IUserService users) : ControllerBase
{
    /// <summary>Мій профіль.</summary>
    [HttpGet]
    public Task<UserDto> Get() => users.GetAsync(User.GetUserId());

    /// <summary>Оновити ім'я та телефон.</summary>
    [HttpPut]
    [ProducesResponseType(typeof(UserDto), 200)]
    [ProducesResponseType(typeof(ProblemDetails), 400)]
    public Task<UserDto> Update(UpdateProfileDto dto) => users.UpdateProfileAsync(User.GetUserId(), dto);

    /// <summary>Змінити пароль (потрібен поточний).</summary>
    [HttpPost("change-password")]
    [ProducesResponseType(204)]
    [ProducesResponseType(typeof(ProblemDetails), 400)]
    public async Task<IActionResult> ChangePassword(ChangePasswordDto dto)
    {
        await users.ChangePasswordAsync(User.GetUserId(), dto);
        return NoContent();
    }
}

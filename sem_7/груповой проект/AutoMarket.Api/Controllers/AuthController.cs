using AutoMarket.Api.Dtos;
using AutoMarket.Api.Extensions;
using AutoMarket.Api.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace AutoMarket.Api.Controllers;

/// <summary>Реєстрація та вхід. Повертає JWT токен.</summary>
[ApiController]
[Route("api/auth")]
[Produces("application/json")]
[EnableRateLimiting(ServiceCollectionExtensions.AuthRateLimitPolicy)]
public class AuthController(IAuthService auth) : ControllerBase
{
    /// <summary>Реєстрація нового користувача (роль User).</summary>
    /// <response code="200">Успіх, повертає токен</response>
    /// <response code="400">Невалідні дані</response>
    /// <response code="409">Email вже зайнятий</response>
    [HttpPost("register")]
    [ProducesResponseType(typeof(AuthResponse), 200)]
    [ProducesResponseType(typeof(ProblemDetails), 400)]
    [ProducesResponseType(typeof(ProblemDetails), 409)]
    public async Task<ActionResult<AuthResponse>> Register(RegisterDto dto) => await auth.RegisterAsync(dto);

    /// <summary>Вхід за email та паролем.</summary>
    /// <response code="200">Успіх, повертає токен</response>
    /// <response code="401">Невірний email або пароль</response>
    /// <response code="403">Користувача заблоковано</response>
    [HttpPost("login")]
    [ProducesResponseType(typeof(AuthResponse), 200)]
    [ProducesResponseType(typeof(ProblemDetails), 401)]
    [ProducesResponseType(typeof(ProblemDetails), 403)]
    public async Task<ActionResult<AuthResponse>> Login(LoginDto dto) => await auth.LoginAsync(dto);
}

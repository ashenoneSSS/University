using AutoMarket.Api.Dtos;
using AutoMarket.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AutoMarket.Api.Controllers.Admin;

/// <summary>Статистика для панелі адміністратора.</summary>
[ApiController]
[Route("api/admin/stats")]
[Authorize(Roles = "Admin")]
[Produces("application/json")]
public class AdminStatsController(IStatsService stats) : ControllerBase
{
    /// <summary>Користувачі, оголошення за статусами й категоріями, середня ціна.</summary>
    [HttpGet]
    public Task<StatsDto> Get() => stats.GetAsync();
}

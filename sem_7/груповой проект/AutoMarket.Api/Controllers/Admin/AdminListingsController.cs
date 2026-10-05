using AutoMarket.Api.Common;
using AutoMarket.Api.Dtos;
using AutoMarket.Api.Models;
using AutoMarket.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AutoMarket.Api.Controllers.Admin;

/// <summary>Модерація оголошень (Admin).</summary>
[ApiController]
[Route("api/admin/listings")]
[Authorize(Roles = "Admin")]
[Produces("application/json")]
public class AdminListingsController(IListingService listings) : ControllerBase
{
    /// <summary>Оголошення за статусом. За замовчуванням — черга модерації (Pending), старіші першими.</summary>
    [HttpGet]
    public Task<PagedResult<ListingDto>> Get([FromQuery] ListingStatus status = ListingStatus.Pending, [FromQuery] PageRequest? page = null) =>
        listings.GetForModerationAsync(status, page ?? new PageRequest());

    /// <summary>Схвалити оголошення → Active.</summary>
    [HttpPost("{id:int}/approve")]
    [ProducesResponseType(typeof(ListingDto), 200)]
    [ProducesResponseType(typeof(ProblemDetails), 400)]
    [ProducesResponseType(typeof(ProblemDetails), 404)]
    public Task<ListingDto> Approve(int id) => listings.ApproveAsync(id);

    /// <summary>Відхилити оголошення → Rejected (причина обов'язкова).</summary>
    [HttpPost("{id:int}/reject")]
    [ProducesResponseType(typeof(ListingDto), 200)]
    [ProducesResponseType(typeof(ProblemDetails), 400)]
    [ProducesResponseType(typeof(ProblemDetails), 404)]
    public Task<ListingDto> Reject(int id, RejectDto dto) => listings.RejectAsync(id, dto.Reason);

    /// <summary>Видалити будь-яке оголошення (порушення правил).</summary>
    [HttpDelete("{id:int}")]
    [ProducesResponseType(204)]
    [ProducesResponseType(typeof(ProblemDetails), 404)]
    public async Task<IActionResult> Delete(int id)
    {
        await listings.AdminDeleteAsync(id);
        return NoContent();
    }
}

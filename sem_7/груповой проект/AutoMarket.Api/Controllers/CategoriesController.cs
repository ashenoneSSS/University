using AutoMarket.Api.Dtos;
using AutoMarket.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AutoMarket.Api.Controllers;

/// <summary>Категорії оголошень. Читання — всім, зміни — лише адміністратору.</summary>
[ApiController]
[Route("api/categories")]
[Produces("application/json")]
public class CategoriesController(ICategoryService categories) : ControllerBase
{
    /// <summary>Усі категорії з кількістю активних оголошень.</summary>
    [HttpGet]
    public Task<List<CategoryDto>> GetAll() => categories.GetAllAsync();

    /// <summary>Категорія за id.</summary>
    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(CategoryDto), 200)]
    [ProducesResponseType(typeof(ProblemDetails), 404)]
    public Task<CategoryDto> Get(int id) => categories.GetAsync(id);

    /// <summary>Створити категорію (Admin).</summary>
    [HttpPost, Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(CategoryDto), 201)]
    [ProducesResponseType(typeof(ProblemDetails), 409)]
    public async Task<ActionResult<CategoryDto>> Create(CategoryCreateDto dto)
    {
        var created = await categories.CreateAsync(dto);
        return CreatedAtAction(nameof(Get), new { id = created.Id }, created);
    }

    /// <summary>Перейменувати категорію (Admin).</summary>
    [HttpPut("{id:int}"), Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(CategoryDto), 200)]
    [ProducesResponseType(typeof(ProblemDetails), 404)]
    [ProducesResponseType(typeof(ProblemDetails), 409)]
    public Task<CategoryDto> Update(int id, CategoryCreateDto dto) => categories.UpdateAsync(id, dto);

    /// <summary>Видалити порожню категорію (Admin).</summary>
    [HttpDelete("{id:int}"), Authorize(Roles = "Admin")]
    [ProducesResponseType(204)]
    [ProducesResponseType(typeof(ProblemDetails), 409)]
    public async Task<IActionResult> Delete(int id)
    {
        await categories.DeleteAsync(id);
        return NoContent();
    }
}

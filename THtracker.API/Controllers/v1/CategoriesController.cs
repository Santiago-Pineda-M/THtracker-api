using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MediatR;
using THtracker.API.Extensions;
using THtracker.Application.Features.Categories;
using THtracker.Application.Features.Categories.Commands.CreateCategory;
using THtracker.Application.Features.Categories.Commands.UpdateCategory;
using THtracker.Application.Features.Categories.Commands.DeleteCategory;
using THtracker.Application.Features.Categories.Queries.GetCategoryById;
using THtracker.Application.Features.Categories.Queries.GetAllCategories;

namespace THtracker.API.Controllers.v1;

/// <summary>
/// Categorías de actividades del sistema.
/// </summary>
[Authorize]
[ApiController]
[Route("categories")]
public sealed class CategoriesController : AuthorizedControllerBase
{
    private readonly ISender _sender;

    public CategoriesController(ISender sender)
    {
        _sender = sender;
    }

    /// <summary>
    /// Obtiene todas las categorías del usuario autenticado.
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<CategoryResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll(CancellationToken cancellationToken)
    {
        var userId = GetUserId();
        var categories = await _sender.Send(new GetAllCategoriesQuery(userId), cancellationToken);
        return Ok(categories);
    }

    /// <summary>
    /// Obtiene una categoría específica por ID.
    /// </summary>
    /// <param name="id">ID de la categoría.</param>
    /// <param name="cancellationToken">Token de cancelación.</param>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(CategoryResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
    {
        var userId = GetUserId();
        var result = await _sender.Send(new GetCategoryByIdQuery(id, userId), cancellationToken);
        return result.ToActionResult();
    }

    /// <summary>
    /// Crea una nueva categoría para el usuario autenticado.
    /// </summary>
    /// <param name="command">Nombre de la categoría.</param>
    /// <param name="ct">Token de cancelación.</param>
    [HttpPost]
    [ProducesResponseType(typeof(CategoryResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create([FromBody] CreateCategoryCommand command, CancellationToken ct)
    {
        var userId = GetUserId();
        var result = await _sender.Send(command with { UserId = userId }, ct);
        
        if (result.IsSuccess)
        {
            return CreatedAtAction(nameof(GetById), new { id = result.Value.Id }, result.Value);
        }

        return result.ToActionResult();
    }

    /// <summary>
    /// Actualiza una categoría (solo dueño).
    /// </summary>
    /// <param name="id">ID de la categoría.</param>
    /// <param name="command">Nuevos datos.</param>
    /// <param name="ct">Token de cancelación.</param>
    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(CategoryResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateCategoryCommand command, CancellationToken ct)
    {
        var userId = GetUserId();
        var result = await _sender.Send(command with { Id = id, UserId = userId }, ct);
        return result.ToActionResult();
    }

    /// <summary>
    /// Elimina una categoría (solo dueño).
    /// </summary>
    /// <param name="id">ID de la categoría.</param>
    /// <param name="ct">Token de cancelación.</param>
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
    {
        var userId = GetUserId();
        var result = await _sender.Send(new DeleteCategoryCommand(id, userId), ct);
        return result.ToActionResult();
    }
}

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using THtracker.API.DTOs;
using THtracker.Application.DTOs.Categories;
using THtracker.Application.UseCases.Categories;

namespace THtracker.API.Controllers;

/// <summary>
/// Categorías del usuario autenticado (CRUD por dueño).
/// </summary>
[Authorize]
[ApiController]
[Route("api/v1/categories")]
public class CategoriesController : AuthorizedControllerBase
{
    private readonly GetAllCategoriesUseCase _getAllCategories;
    private readonly GetCategoryByIdUseCase _getCategoryById;
    private readonly CreateCategoryUseCase _createCategory;
    private readonly UpdateCategoryUseCase _updateCategory;
    private readonly DeleteCategoryUseCase _deleteCategory;

    public CategoriesController(
        GetAllCategoriesUseCase getAllCategories,
        GetCategoryByIdUseCase getCategoryById,
        CreateCategoryUseCase createCategory,
        UpdateCategoryUseCase updateCategory,
        DeleteCategoryUseCase deleteCategory
    )
    {
        _getAllCategories = getAllCategories;
        _getCategoryById = getCategoryById;
        _createCategory = createCategory;
        _updateCategory = updateCategory;
        _deleteCategory = deleteCategory;
    }

    /// <summary>
    /// Lista las categorías del usuario autenticado.
    /// </summary>
    /// <param name="cancellationToken">Token de cancelación.</param>
    /// <returns>Lista de categorías.</returns>
    /// <response code="200">Lista de categorías.</response>
    /// <response code="401">No autenticado.</response>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<CategoryResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetAll(CancellationToken cancellationToken)
    {
        var userId = GetUserId();
        var categories = await _getAllCategories.ExecuteAsync(userId, cancellationToken);
        return Ok(categories);
    }

    /// <summary>
    /// Obtiene una categoría por ID (solo si es del usuario).
    /// </summary>
    /// <param name="id">ID de la categoría.</param>
    /// <returns>Categoría.</returns>
    /// <response code="200">Categoría.</response>
    /// <response code="401">No autenticado.</response>
    /// <response code="403">No es dueño de la categoría.</response>
    /// <response code="404">Categoría no encontrada.</response>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(CategoryResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(Guid id)
    {
        var category = await _getCategoryById.ExecuteAsync(id);
        if (category == null) return NotFound();

        var userId = GetUserId();
        if (category.UserId != userId) return Forbid();

        return Ok(category);
    }

    /// <summary>
    /// Crea una nueva categoría para el usuario autenticado.
    /// </summary>
    /// <param name="request">Nombre de la categoría.</param>
    /// <returns>Categoría creada y Location.</returns>
    /// <response code="201">Categoría creada.</response>
    /// <response code="400">Datos inválidos.</response>
    /// <response code="401">No autenticado.</response>
    [HttpPost]
    [ProducesResponseType(typeof(CategoryResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Create([FromBody] CreateCategoryRequest request, CancellationToken cancellationToken)
    {
        try
        {
            var userId = GetUserId();
            var category = await _createCategory.ExecuteAsync(userId, request, cancellationToken);
            return CreatedAtAction(nameof(GetById), new { id = category.Id }, category);
        }
        catch (Exception ex)
        {
            return BadRequest(new ApiErrorResponse(ex.Message));
        }
    }

    /// <summary>
    /// Actualiza una categoría (solo dueño).
    /// </summary>
    /// <param name="id">ID de la categoría.</param>
    /// <param name="request">Nuevo nombre.</param>
    /// <returns>Categoría actualizada.</returns>
    /// <response code="200">Categoría actualizada.</response>
    /// <response code="401">No autenticado.</response>
    /// <response code="403">No es dueño.</response>
    /// <response code="404">Categoría no encontrada.</response>
    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(CategoryResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateCategoryRequest request)
    {
        var existing = await _getCategoryById.ExecuteAsync(id);
        if (existing == null) return NotFound();

        var userId = GetUserId();
        if (existing.UserId != userId) return Forbid();

        var category = await _updateCategory.ExecuteAsync(id, request);
        return Ok(category);
    }

    /// <summary>
    /// Elimina una categoría (solo dueño).
    /// </summary>
    /// <param name="id">ID de la categoría.</param>
    /// <response code="204">Eliminada.</response>
    /// <response code="401">No autenticado.</response>
    /// <response code="403">No es dueño.</response>
    /// <response code="404">Categoría no encontrada.</response>
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(Guid id)
    {
        var existing = await _getCategoryById.ExecuteAsync(id);
        if (existing == null) return NotFound();

        var userId = GetUserId();
        if (existing.UserId != userId) return Forbid();

        var deleted = await _deleteCategory.ExecuteAsync(id);
        return deleted ? NoContent() : NotFound();
    }
}

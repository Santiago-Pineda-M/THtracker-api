using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using THtracker.API.Extensions;
using THtracker.Application.DTOs.Categories;
using THtracker.Application.UseCases.Categories;

namespace THtracker.API.Controllers;

/// <summary>
/// Categorías de actividades del sistema.
/// </summary>
[Authorize]
[ApiController]
[Route("api/v1/categories")]
public class CategoriesController : AuthorizedControllerBase
{
    private readonly GetAllCategoriesUseCase _getCategoriesByUser;
    private readonly GetCategoryByIdUseCase _getCategoryById;
    private readonly CreateCategoryUseCase _createCategory;
    private readonly UpdateCategoryUseCase _updateCategory;
    private readonly DeleteCategoryUseCase _deleteCategory;

    public CategoriesController(
        GetAllCategoriesUseCase getCategoriesByUser,
        GetCategoryByIdUseCase getCategoryById,
        CreateCategoryUseCase createCategory,
        UpdateCategoryUseCase updateCategory,
        DeleteCategoryUseCase deleteCategory
    )
    {
        _getCategoriesByUser = getCategoriesByUser;
        _getCategoryById = getCategoryById;
        _createCategory = createCategory;
        _updateCategory = updateCategory;
        _deleteCategory = deleteCategory;
    }

    /// <summary>
    /// Obtiene todas las categorías del usuario autenticado.
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<CategoryResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll(CancellationToken cancellationToken)
    {
        var userId = GetUserId();
        var categories = await _getCategoriesByUser.ExecuteAsync(userId, cancellationToken);
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
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
    {
        var userId = GetUserId();
        var result = await _getCategoryById.ExecuteAsync(userId, id, cancellationToken);
        return result.ToActionResult();
    }

    /// <summary>
    /// Crea una nueva categoría para el usuario autenticado.
    /// </summary>
    /// <param name="request">Nombre de la categoría.</param>
    [HttpPost]
    [ProducesResponseType(typeof(CategoryResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create([FromBody] CreateCategoryRequest request)
    {
        var userId = GetUserId();
        var result = await _createCategory.ExecuteAsync(userId, request);
        
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
    /// <param name="request">Nuevo nombre.</param>
    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(CategoryResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateCategoryRequest request)
    {
        var userId = GetUserId();
        var result = await _updateCategory.ExecuteAsync(userId, id, request);
        return result.ToActionResult();
    }

    /// <summary>
    /// Elimina una categoría (solo dueño).
    /// </summary>
    /// <param name="id">ID de la categoría.</param>
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(Guid id)
    {
        var userId = GetUserId();
        var result = await _deleteCategory.ExecuteAsync(userId, id);
        return result.ToActionResult();
    }
}

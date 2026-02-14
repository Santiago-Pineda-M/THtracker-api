using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using THtracker.Application.DTOs.Categories;
using THtracker.Application.UseCases.Categories;

namespace THtracker.API.Controllers;

[Authorize]
[ApiController]
[Route("api/v1/categories")]
public class CategoriesController : ControllerBase
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

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var userId = GetUserId();
        var categories = await _getAllCategories.ExecuteAsync(userId);
        return Ok(categories);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var category = await _getCategoryById.ExecuteAsync(id);
        if (category == null) return NotFound();
        
        // Security check: Only owner can see the category
        var userId = GetUserId();
        if (category.UserId != userId) return Forbid();

        return Ok(category);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateCategoryRequest request)
    {
        var userId = GetUserId();
        var category = await _createCategory.ExecuteAsync(userId, request);
        return CreatedAtAction(nameof(GetById), new { id = category.Id }, category);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateCategoryRequest request)
    {
        var existing = await _getCategoryById.ExecuteAsync(id);
        if (existing == null) return NotFound();

        // Security check
        var userId = GetUserId();
        if (existing.UserId != userId) return Forbid();

        var category = await _updateCategory.ExecuteAsync(id, request);
        return Ok(category);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var existing = await _getCategoryById.ExecuteAsync(id);
        if (existing == null) return NotFound();

        // Security check
        var userId = GetUserId();
        if (existing.UserId != userId) return Forbid();

        var deleted = await _deleteCategory.ExecuteAsync(id);
        return deleted ? NoContent() : NotFound();
    }

    private Guid GetUserId()
    {
        var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (userIdClaim == null || !Guid.TryParse(userIdClaim, out var userId))
        {
            throw new UnauthorizedAccessException("User ID claim not found.");
        }
        return userId;
    }
}

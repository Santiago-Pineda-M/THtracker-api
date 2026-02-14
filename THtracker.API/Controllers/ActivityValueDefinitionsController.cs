using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using THtracker.Application.DTOs.ActivityValueDefinitions;
using THtracker.Application.UseCases.ActivityValueDefinitions;

namespace THtracker.API.Controllers;

[Authorize]
[ApiController]
[Route("api/v1/activities/{activityId}/definitions")]
public class ActivityValueDefinitionsController : ControllerBase
{
    private readonly CreateValueDefinitionUseCase _createDefinition;
    private readonly GetValueDefinitionsUseCase _getDefinitions;

    public ActivityValueDefinitionsController(
        CreateValueDefinitionUseCase createDefinition,
        GetValueDefinitionsUseCase getDefinitions)
    {
        _createDefinition = createDefinition;
        _getDefinitions = getDefinitions;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll(Guid activityId)
    {
        try
        {
            var userId = GetUserId();
            var definitions = await _getDefinitions.ExecuteAsync(userId, activityId);
            return Ok(definitions);
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPost]
    public async Task<IActionResult> Create(Guid activityId, [FromBody] CreateValueDefinitionRequest request)
    {
        try
        {
            var userId = GetUserId();
            var definition = await _createDefinition.ExecuteAsync(userId, activityId, request);
            return CreatedAtAction(nameof(GetAll), new { activityId }, definition);
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
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

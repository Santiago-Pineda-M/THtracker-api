using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using THtracker.Application.DTOs.Activities;
using THtracker.Application.UseCases.Activities;

namespace THtracker.API.Controllers;

[Authorize]
[ApiController]
[Route("api/v1/activities")]
public class ActivitiesController : ControllerBase
{
    private readonly GetAllActivitiesUseCase _getAllActivities;
    private readonly GetActivityByIdUseCase _getActivityById;
    private readonly CreateActivityUseCase _createActivity;
    private readonly UpdateActivityUseCase _updateActivity;
    private readonly DeleteActivityUseCase _deleteActivity;

    public ActivitiesController(
        GetAllActivitiesUseCase getAllActivities,
        GetActivityByIdUseCase getActivityById,
        CreateActivityUseCase createActivity,
        UpdateActivityUseCase updateActivity,
        DeleteActivityUseCase deleteActivity
    )
    {
        _getAllActivities = getAllActivities;
        _getActivityById = getActivityById;
        _createActivity = createActivity;
        _updateActivity = updateActivity;
        _deleteActivity = deleteActivity;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var userId = GetUserId();
        var activities = await _getAllActivities.ExecuteAsync(userId);
        return Ok(activities);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var activity = await _getActivityById.ExecuteAsync(id);
        if (activity == null) return NotFound();

        var userId = GetUserId();
        if (activity.UserId != userId) return Forbid();

        return Ok(activity);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateActivityRequest request)
    {
        try
        {
            var userId = GetUserId();
            var activity = await _createActivity.ExecuteAsync(userId, request);
            return CreatedAtAction(nameof(GetById), new { id = activity.Id }, activity);
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateActivityRequest request)
    {
        try
        {
            var existing = await _getActivityById.ExecuteAsync(id);
            if (existing == null) return NotFound();

            var userId = GetUserId();
            if (existing.UserId != userId) return Forbid();

            var activity = await _updateActivity.ExecuteAsync(id, request);
            return Ok(activity);
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var existing = await _getActivityById.ExecuteAsync(id);
        if (existing == null) return NotFound();

        var userId = GetUserId();
        if (existing.UserId != userId) return Forbid();

        var deleted = await _deleteActivity.ExecuteAsync(id);
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

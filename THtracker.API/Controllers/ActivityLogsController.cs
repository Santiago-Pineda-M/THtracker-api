using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using THtracker.Application.DTOs.ActivityLogs;
using THtracker.Application.DTOs.ActivityLogValues;
using THtracker.Application.UseCases.ActivityLogs;
using THtracker.Application.UseCases.ActivityLogValues;

namespace THtracker.API.Controllers;

[Authorize]
[ApiController]
[Route("api/v1/activity-logs")]
public class ActivityLogsController : ControllerBase
{
    private readonly StartActivityUseCase _startActivity;
    private readonly StopActivityUseCase _stopActivity;
    private readonly UpdateActivityLogUseCase _updateActivityLog;
    private readonly SaveLogValuesUseCase _saveValues;

    public ActivityLogsController(
        StartActivityUseCase startActivity, 
        StopActivityUseCase stopActivity,
        UpdateActivityLogUseCase updateActivityLog,
        SaveLogValuesUseCase saveValues)
    {
        _startActivity = startActivity;
        _stopActivity = stopActivity;
        _updateActivityLog = updateActivityLog;
        _saveValues = saveValues;
    }

    [HttpPost("start")]
    public async Task<IActionResult> Start([FromBody] StartActivityLogRequest request)
    {
        try
        {
            var userId = GetUserId();
            var log = await _startActivity.ExecuteAsync(userId, request);
            return Ok(log);
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPost("{id}/stop")]
    public async Task<IActionResult> Stop(Guid id)
    {
        try
        {
            var userId = GetUserId();
            var log = await _stopActivity.ExecuteAsync(userId, id);
            return log == null ? NotFound() : Ok(log);
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateActivityLogRequest request)
    {
        try
        {
            var userId = GetUserId();
            var log = await _updateActivityLog.ExecuteAsync(userId, id, request);
            return log == null ? NotFound() : Ok(log);
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPost("{id}/values")]
    public async Task<IActionResult> AddValues(Guid id, [FromBody] IEnumerable<LogValueRequest> values)
    {
        try
        {
            var userId = GetUserId();
            var result = await _saveValues.ExecuteAsync(userId, id, values);
            return Ok(result);
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

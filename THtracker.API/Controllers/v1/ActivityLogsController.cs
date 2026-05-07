using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MediatR;
using THtracker.API.Extensions;
using THtracker.Application.Common;
using THtracker.Application.Interfaces;
using THtracker.Application.Features.ActivityLogs;
using THtracker.Application.Features.ActivityLogs.Commands.StartActivity;
using THtracker.Application.Features.ActivityLogs.Commands.StopActivity;
using THtracker.Application.Features.ActivityLogs.Commands.UpdateActivityLog;
using THtracker.Application.Features.ActivityLogs.Queries.GetActivityLogs;
using THtracker.Application.Features.ActivityLogs.Queries.GetActiveActivityLogs;
using THtracker.Application.Features.ActivityLogValues;
using THtracker.Application.Features.ActivityLogValues.Commands.SaveLogValues;
using THtracker.Application.Features.ActivityLogValues.Queries.GetLogValues;
using THtracker.Application.Features.ActivityLogs.Queries.GetActivityLogById;

namespace THtracker.API.Controllers.v1;

/// <summary>
/// Registros de actividad (inicio/parada, actualización, valores y consulta).
/// </summary>
[Authorize]
[ApiController]
[Route("activity-logs")]
public sealed class ActivityLogsController : AuthorizedControllerBase
{
    private readonly ISender _sender;

    public ActivityLogsController(ICurrentUserService currentUser, ISender sender) : base(currentUser)
    {
        _sender = sender;
    }

    /// <summary>
    /// Lista los registros de actividad con filtros opcionales.
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(PaginatedResponse<ActivityLogResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll(
        [FromQuery] Guid? activityId,
        [FromQuery] DateTime? from,
        [FromQuery] DateTime? to,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = Pagination.DefaultPageSize,
        CancellationToken ct = default)
    {
        var userId = GetUserId();
        var result = await _sender.Send(new GetActivityLogsQuery(userId, activityId, from, to, pageNumber, pageSize), ct);
        return result.ToActionResult();
    }

    /// <summary>
    /// Lista los registros de actividad que están en curso.
    /// </summary>
    [HttpGet("active")]
    [ProducesResponseType(typeof(PaginatedResponse<ActivityLogResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetActive(
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = Pagination.DefaultPageSize,
        CancellationToken ct = default)
    {
        var userId = GetUserId();
        var result = await _sender.Send(new GetActiveActivityLogsQuery(userId, pageNumber, pageSize), ct);
        return result.ToActionResult();
    }

    /// <summary>
    /// Obtiene un registro de actividad por ID.
    /// </summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(ActivityLogResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(Guid id, CancellationToken ct)
    {
        var userId = GetUserId();
        var result = await _sender.Send(new GetActivityLogByIdQuery(id, userId), ct);
        return result.ToActionResult();
    }

    /// <summary>
    /// Inicia un registro de actividad (crea un nuevo log).
    /// </summary>
    [HttpPost("start")]
    [ProducesResponseType(typeof(ActivityLogResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Start([FromBody] StartActivityCommand command, CancellationToken ct)
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
    /// Detiene un registro de actividad en curso.
    /// </summary>
    [HttpPost("{id:guid}/stop")]
    [ProducesResponseType(typeof(ActivityLogResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Stop(Guid id, CancellationToken ct)
    {
        var userId = GetUserId();
        var result = await _sender.Send(new StopActivityCommand(id, userId), ct);
        return result.ToActionResult();
    }

    /// <summary>
    /// Actualiza un registro de actividad.
    /// </summary>
    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(ActivityLogResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateActivityLogCommand command, CancellationToken ct)
    {
        var userId = GetUserId();
        var result = await _sender.Send(command with { Id = id, UserId = userId }, ct);
        return result.ToActionResult();
    }

    /// <summary>
    /// Guarda los valores personalizados para un registro de actividad.
    /// </summary>
    [HttpPost("{id:guid}/values")]
    [ProducesResponseType(typeof(IEnumerable<LogValueResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> SaveValues(Guid id, [FromBody] SaveLogValuesCommand command, CancellationToken ct)
    {
        var userId = GetUserId();
        var result = await _sender.Send(command with { ActivityLogId = id, UserId = userId }, ct);
        return result.ToActionResult();
    }

    /// <summary>
    /// Obtiene los valores personalizados de un registro de actividad.
    /// </summary>
    [HttpGet("{id:guid}/values")]
    [ProducesResponseType(typeof(PaginatedResponse<LogValueResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetValues(
        Guid id,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = Pagination.DefaultPageSize,
        CancellationToken ct = default)
    {
        var userId = GetUserId();
        var result = await _sender.Send(new GetLogValuesQuery(id, userId, pageNumber, pageSize), ct);
        return result.ToActionResult();
    }
}
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using THtracker.API.Extensions;
using THtracker.Application.DTOs.ActivityLogs;
using THtracker.Application.DTOs.ActivityLogValues;
using THtracker.Application.UseCases.ActivityLogs;
using THtracker.Application.UseCases.ActivityLogValues;

namespace THtracker.API.Controllers;

/// <summary>
/// Registros de actividad (inicio/parada, actualización, valores y consulta).
/// </summary>
[Authorize]
[ApiController]
[Route("api/v1/activity-logs")]
public class ActivityLogsController : AuthorizedControllerBase
{
    private readonly GetActivityLogsByActivityUseCase _getLogsByActivity;
    private readonly GetActivityLogByIdUseCase _getLogById;
    private readonly StartActivityUseCase _startActivity;
    private readonly StopActivityUseCase _stopActivity;
    private readonly UpdateActivityLogUseCase _updateActivityLog;
    private readonly SaveLogValuesUseCase _saveValues;
    private readonly GetLogValuesUseCase _getLogValues;

    public ActivityLogsController(
        GetActivityLogsByActivityUseCase getLogsByActivity,
        GetActivityLogByIdUseCase getLogById,
        StartActivityUseCase startActivity,
        StopActivityUseCase stopActivity,
        UpdateActivityLogUseCase updateActivityLog,
        SaveLogValuesUseCase saveValues,
        GetLogValuesUseCase getLogValues
    )
    {
        _getLogsByActivity = getLogsByActivity;
        _getLogById = getLogById;
        _startActivity = startActivity;
        _stopActivity = stopActivity;
        _updateActivityLog = updateActivityLog;
        _saveValues = saveValues;
        _getLogValues = getLogValues;
    }

    /// <summary>
    /// Lista los registros de actividad de una actividad (solo dueño).
    /// </summary>
    /// <param name="activityId">ID de la actividad (query).</param>
    /// <param name="cancellationToken">Token de cancelación.</param>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<ActivityLogResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll(
        [FromQuery] Guid activityId,
        CancellationToken cancellationToken
    )
    {
        var userId = GetUserId();
        var result = await _getLogsByActivity.ExecuteAsync(userId, activityId, cancellationToken);
        return result.ToActionResult();
    }

    /// <summary>
    /// Obtiene un registro de actividad por ID.
    /// </summary>
    /// <param name="id">ID del registro (log).</param>
    /// <param name="cancellationToken">Token de cancelación.</param>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(ActivityLogResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
    {
        var userId = GetUserId();
        var result = await _getLogById.ExecuteAsync(userId, id, cancellationToken);
        return result.ToActionResult();
    }

    /// <summary>
    /// Inicia un registro de actividad (crea un nuevo log).
    /// </summary>
    /// <param name="request">ID de la actividad a registrar.</param>
    /// <param name="cancellationToken">Token de cancelación.</param>
    [HttpPost("start")]
    [ProducesResponseType(typeof(ActivityLogResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Start(
        [FromBody] StartActivityLogRequest request,
        CancellationToken cancellationToken
    )
    {
        var userId = GetUserId();
        var result = await _startActivity.ExecuteAsync(userId, request, cancellationToken);
        
        if (result.IsSuccess)
        {
            return CreatedAtAction(nameof(GetById), new { id = result.Value.Id }, result.Value);
        }

        return result.ToActionResult();
    }

    /// <summary>
    /// Detiene un registro de actividad en curso.
    /// </summary>
    /// <param name="id">ID del registro (log).</param>
    /// <param name="cancellationToken">Token de cancelación.</param>
    [HttpPost("{id:guid}/stop")]
    [ProducesResponseType(typeof(ActivityLogResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Stop(Guid id, CancellationToken cancellationToken)
    {
        var userId = GetUserId();
        var result = await _stopActivity.ExecuteAsync(userId, id, cancellationToken);
        return result.ToActionResult();
    }

    /// <summary>
    /// Actualiza metadatos de un registro de actividad.
    /// </summary>
    /// <param name="id">ID del registro.</param>
    /// <param name="request">Datos a actualizar.</param>
    /// <param name="cancellationToken">Token de cancelación.</param>
    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(ActivityLogResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Update(
        Guid id,
        [FromBody] UpdateActivityLogRequest request,
        CancellationToken cancellationToken
    )
    {
        var userId = GetUserId();
        var result = await _updateActivityLog.ExecuteAsync(userId, id, request, cancellationToken);
        return result.ToActionResult();
    }

    /// <summary>
    /// Guarda o actualiza valores medidos en un registro (ej. cantidad, notas).
    /// </summary>
    /// <param name="id">ID del registro.</param>
    /// <param name="values">Lista de valores (definitionId + value).</param>
    /// <param name="cancellationToken">Token de cancelación.</param>
    [HttpPost("{id:guid}/values")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> AddValues(
        Guid id,
        [FromBody] IEnumerable<LogValueRequest> values,
        CancellationToken cancellationToken
    )
    {
        var userId = GetUserId();
        var result = await _saveValues.ExecuteAsync(userId, id, values, cancellationToken);
        return result.ToActionResult();
    }

    /// <summary>
    /// Obtiene los valores personalizados registrados en un registro de actividad.
    /// </summary>
    /// <param name="id">ID del registro (log).</param>
    /// <param name="cancellationToken">Token de cancelación.</param>
    [HttpGet("{id:guid}/values")]
    [ProducesResponseType(typeof(IEnumerable<LogValueResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetValues(Guid id, CancellationToken cancellationToken)
    {
        var userId = GetUserId();
        var result = await _getLogValues.ExecuteAsync(userId, id, cancellationToken);
        return result.ToActionResult();
    }
}

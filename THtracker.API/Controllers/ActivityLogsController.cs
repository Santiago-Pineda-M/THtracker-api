using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using THtracker.API.DTOs;
using THtracker.Application.DTOs.ActivityLogs;
using THtracker.Application.DTOs.ActivityLogValues;
using THtracker.Application.UseCases.ActivityLogs;
using THtracker.Application.UseCases.ActivityLogValues;

namespace THtracker.API.Controllers;

/// <summary>
/// Registros de actividad (inicio/parada, actualización, valores y consulta). REST: GET colección por activityId, GET por id, POST start → 201 Created.
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

    public ActivityLogsController(
        GetActivityLogsByActivityUseCase getLogsByActivity,
        GetActivityLogByIdUseCase getLogById,
        StartActivityUseCase startActivity,
        StopActivityUseCase stopActivity,
        UpdateActivityLogUseCase updateActivityLog,
        SaveLogValuesUseCase saveValues
    )
    {
        _getLogsByActivity = getLogsByActivity;
        _getLogById = getLogById;
        _startActivity = startActivity;
        _stopActivity = stopActivity;
        _updateActivityLog = updateActivityLog;
        _saveValues = saveValues;
    }

    /// <summary>
    /// Lista los registros de actividad de una actividad (solo dueño).
    /// </summary>
    /// <param name="activityId">ID de la actividad (query).</param>
    /// <param name="cancellationToken">Token de cancelación.</param>
    /// <returns>Lista de registros (logs) de esa actividad.</returns>
    /// <response code="200">Lista de registros.</response>
    /// <response code="401">No autenticado.</response>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<ActivityLogResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetAll(
        [FromQuery] Guid activityId,
        CancellationToken cancellationToken
    )
    {
        var userId = GetUserId();
        var logs = await _getLogsByActivity.ExecuteAsync(userId, activityId, cancellationToken);
        return Ok(logs);
    }

    /// <summary>
    /// Obtiene un registro de actividad por ID (solo si pertenece a una actividad del usuario).
    /// </summary>
    /// <param name="id">ID del registro (log).</param>
    /// <param name="cancellationToken">Token de cancelación.</param>
    /// <returns>Registro con fechas y duración.</returns>
    /// <response code="200">Registro.</response>
    /// <response code="401">No autenticado.</response>
    /// <response code="404">Registro no encontrado o sin acceso.</response>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(ActivityLogResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
    {
        var userId = GetUserId();
        var log = await _getLogById.ExecuteAsync(userId, id, cancellationToken);
        return log == null ? NotFound() : Ok(log);
    }

    /// <summary>
    /// Inicia un registro de actividad (crea un nuevo log).
    /// </summary>
    /// <param name="request">ID de la actividad a registrar.</param>
    /// <param name="cancellationToken">Token de cancelación.</param>
    /// <returns>Registro creado y cabecera Location a GET por ID.</returns>
    /// <response code="201">Registro iniciado.</response>
    /// <response code="400">Datos inválidos o actividad ya en curso.</response>
    /// <response code="401">No autenticado.</response>
    [HttpPost("start")]
    [ProducesResponseType(typeof(ActivityLogResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Start(
        [FromBody] StartActivityLogRequest request,
        CancellationToken cancellationToken
    )
    {
        try
        {
            var userId = GetUserId();
            var log = await _startActivity.ExecuteAsync(userId, request, cancellationToken);
            return CreatedAtAction(nameof(GetById), new { id = log.Id }, log);
        }
        catch (Exception ex)
        {
            return BadRequest(new ApiErrorResponse(ex.Message));
        }
    }

    /// <summary>
    /// Detiene un registro de actividad en curso.
    /// </summary>
    /// <param name="id">ID del registro (log).</param>
    /// <param name="cancellationToken">Token de cancelación.</param>
    /// <returns>Registro actualizado con fecha de fin y duración.</returns>
    /// <response code="200">Registro detenido.</response>
    /// <response code="400">Error de negocio (ej. ya finalizado).</response>
    /// <response code="401">No autenticado.</response>
    /// <response code="404">Registro no encontrado o sin acceso.</response>
    [HttpPost("{id:guid}/stop")]
    [ProducesResponseType(typeof(ActivityLogResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Stop(Guid id, CancellationToken cancellationToken)
    {
        try
        {
            var userId = GetUserId();
            var log = await _stopActivity.ExecuteAsync(userId, id, cancellationToken);
            return log == null ? NotFound() : Ok(log);
        }
        catch (Exception ex)
        {
            return BadRequest(new ApiErrorResponse(ex.Message));
        }
    }

    /// <summary>
    /// Actualiza metadatos de un registro de actividad.
    /// </summary>
    /// <param name="id">ID del registro.</param>
    /// <param name="request">Datos a actualizar.</param>
    /// <param name="cancellationToken">Token de cancelación.</param>
    /// <returns>Registro actualizado.</returns>
    /// <response code="200">Registro actualizado.</response>
    /// <response code="400">Datos inválidos.</response>
    /// <response code="401">No autenticado.</response>
    /// <response code="404">Registro no encontrado o sin acceso.</response>
    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(ActivityLogResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(
        Guid id,
        [FromBody] UpdateActivityLogRequest request,
        CancellationToken cancellationToken
    )
    {
        try
        {
            var userId = GetUserId();
            var log = await _updateActivityLog.ExecuteAsync(userId, id, request, cancellationToken);
            return log == null ? NotFound() : Ok(log);
        }
        catch (Exception ex)
        {
            return BadRequest(new ApiErrorResponse(ex.Message));
        }
    }

    /// <summary>
    /// Guarda o actualiza valores medidos en un registro (ej. cantidad, notas).
    /// </summary>
    /// <param name="id">ID del registro.</param>
    /// <param name="values">Lista de valores (definitionId + value).</param>
    /// <param name="cancellationToken">Token de cancelación.</param>
    /// <returns>Resultado del guardado.</returns>
    /// <response code="200">Valores guardados.</response>
    /// <response code="400">Datos inválidos.</response>
    /// <response code="401">No autenticado.</response>
    /// <response code="404">Registro no encontrado o sin acceso.</response>
    [HttpPost("{id:guid}/values")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> AddValues(
        Guid id,
        [FromBody] IEnumerable<LogValueRequest> values,
        CancellationToken cancellationToken
    )
    {
        try
        {
            var userId = GetUserId();
            var result = await _saveValues.ExecuteAsync(userId, id, values, cancellationToken);
            return Ok(result);
        }
        catch (Exception ex)
        {
            return BadRequest(new ApiErrorResponse(ex.Message));
        }
    }
}

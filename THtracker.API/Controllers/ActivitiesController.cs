using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using THtracker.API.DTOs;
using THtracker.Application.DTOs.Activities;
using THtracker.Application.UseCases.Activities;

namespace THtracker.API.Controllers;

/// <summary>
/// Actividades del usuario autenticado (CRUD por dueño).
/// </summary>
[Authorize]
[ApiController]
[Route("api/v1/activities")]
public class ActivitiesController : AuthorizedControllerBase
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

    /// <summary>
    /// Lista las actividades del usuario autenticado.
    /// </summary>
    /// <returns>Lista de actividades.</returns>
    /// <response code="200">Lista de actividades.</response>
    /// <response code="401">No autenticado.</response>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<ActivityResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetAll()
    {
        var userId = GetUserId();
        var activities = await _getAllActivities.ExecuteAsync(userId);
        return Ok(activities);
    }

    /// <summary>
    /// Obtiene una actividad por ID (solo si es del usuario).
    /// </summary>
    /// <param name="id">ID de la actividad.</param>
    /// <returns>Actividad.</returns>
    /// <response code="200">Actividad.</response>
    /// <response code="401">No autenticado.</response>
    /// <response code="403">No es dueño.</response>
    /// <response code="404">Actividad no encontrada.</response>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(ActivityResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(Guid id)
    {
        var activity = await _getActivityById.ExecuteAsync(id);
        if (activity == null) return NotFound();

        var userId = GetUserId();
        if (activity.UserId != userId) return Forbid();

        return Ok(activity);
    }

    /// <summary>
    /// Crea una nueva actividad para el usuario autenticado.
    /// </summary>
    /// <param name="request">Nombre, categoría y opciones.</param>
    /// <returns>Actividad creada y Location.</returns>
    /// <response code="201">Actividad creada.</response>
    /// <response code="400">Datos inválidos.</response>
    /// <response code="401">No autenticado.</response>
    [HttpPost]
    [ProducesResponseType(typeof(ActivityResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
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
            return BadRequest(new ApiErrorResponse(ex.Message));
        }
    }

    /// <summary>
    /// Actualiza una actividad (solo dueño).
    /// </summary>
    /// <param name="id">ID de la actividad.</param>
    /// <param name="request">Nuevos datos.</param>
    /// <returns>Actividad actualizada.</returns>
    /// <response code="200">Actividad actualizada.</response>
    /// <response code="400">Datos inválidos.</response>
    /// <response code="401">No autenticado.</response>
    /// <response code="403">No es dueño.</response>
    /// <response code="404">Actividad no encontrada.</response>
    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(ActivityResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
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
            return BadRequest(new ApiErrorResponse(ex.Message));
        }
    }

    /// <summary>
    /// Elimina una actividad (solo dueño).
    /// </summary>
    /// <param name="id">ID de la actividad.</param>
    /// <response code="204">Eliminada.</response>
    /// <response code="401">No autenticado.</response>
    /// <response code="403">No es dueño.</response>
    /// <response code="404">Actividad no encontrada.</response>
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(Guid id)
    {
        var existing = await _getActivityById.ExecuteAsync(id);
        if (existing == null) return NotFound();

        var userId = GetUserId();
        if (existing.UserId != userId) return Forbid();

        var deleted = await _deleteActivity.ExecuteAsync(id);
        return deleted ? NoContent() : NotFound();
    }
}

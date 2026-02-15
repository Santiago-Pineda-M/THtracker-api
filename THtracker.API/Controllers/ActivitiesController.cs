using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using THtracker.API.Extensions;
using THtracker.Application.DTOs.Activities;
using THtracker.Application.UseCases.Activities;

namespace THtracker.API.Controllers;

/// <summary>
/// Actividades del sistema (CRUD para el usuario autenticado).
/// </summary>
[Authorize]
[ApiController]
[Route("api/v1/activities")]
public class ActivitiesController : AuthorizedControllerBase
{
    private readonly GetAllActivitiesUseCase _getActivitiesByUser;
    private readonly GetActivityByIdUseCase _getActivityById;
    private readonly CreateActivityUseCase _createActivity;
    private readonly UpdateActivityUseCase _updateActivity;
    private readonly DeleteActivityUseCase _deleteActivity;

    public ActivitiesController(
        GetAllActivitiesUseCase getActivitiesByUser,
        GetActivityByIdUseCase getActivityById,
        CreateActivityUseCase createActivity,
        UpdateActivityUseCase updateActivity,
        DeleteActivityUseCase deleteActivity
    )
    {
        _getActivitiesByUser = getActivitiesByUser;
        _getActivityById = getActivityById;
        _createActivity = createActivity;
        _updateActivity = updateActivity;
        _deleteActivity = deleteActivity;
    }

    /// <summary>
    /// Obtiene todas las actividades del usuario autenticado.
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<ActivityResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll(CancellationToken cancellationToken)
    {
        var userId = GetUserId();
        var activities = await _getActivitiesByUser.ExecuteAsync(userId, cancellationToken);
        return Ok(activities);
    }

    /// <summary>
    /// Obtiene una actividad específica por su ID.
    /// </summary>
    /// <param name="id">ID de la actividad.</param>
    /// <param name="cancellationToken">Token de cancelación.</param>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(ActivityResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
    {
        var userId = GetUserId();
        var result = await _getActivityById.ExecuteAsync(userId, id, cancellationToken);
        return result.ToActionResult();
    }

    /// <summary>
    /// Crea una nueva actividad para el usuario autenticado.
    /// </summary>
    /// <param name="request">Nombre, categoría y opciones.</param>
    [HttpPost]
    [ProducesResponseType(typeof(ActivityResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create([FromBody] CreateActivityRequest request)
    {
        var userId = GetUserId();
        var result = await _createActivity.ExecuteAsync(userId, request);
        
        if (result.IsSuccess)
        {
            return CreatedAtAction(nameof(GetById), new { id = result.Value.Id }, result.Value);
        }

        return result.ToActionResult();
    }

    /// <summary>
    /// Actualiza una actividad (solo dueño).
    /// </summary>
    /// <param name="id">ID de la actividad.</param>
    /// <param name="request">Nuevos datos.</param>
    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(ActivityResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateActivityRequest request)
    {
        var userId = GetUserId();
        var result = await _updateActivity.ExecuteAsync(userId, id, request);
        return result.ToActionResult();
    }

    /// <summary>
    /// Elimina una actividad (solo dueño).
    /// </summary>
    /// <param name="id">ID de la actividad.</param>
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(Guid id)
    {
        var userId = GetUserId();
        var result = await _deleteActivity.ExecuteAsync(userId, id);
        return result.ToActionResult();
    }
}

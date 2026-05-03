using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MediatR;
using THtracker.API.Extensions;
using THtracker.Application.Features.Activities;
using THtracker.Application.Features.Activities.Commands.CreateActivity;
using THtracker.Application.Features.Activities.Commands.UpdateActivity;
using THtracker.Application.Features.Activities.Commands.DeleteActivity;
using THtracker.Application.Features.Activities.Queries.GetActivityById;
using THtracker.Application.Features.Activities.Queries.GetAllActivities;

namespace THtracker.API.Controllers.v1;

/// <summary>
/// Actividades del sistema (CRUD para el usuario autenticado).
/// </summary>
[Authorize]
[ApiController]
[Route("activities")]
public sealed class ActivitiesController : AuthorizedControllerBase
{
    private readonly ISender _sender;

    public ActivitiesController(ISender sender)
    {
        _sender = sender;
    }

    /// <summary>
    /// Obtiene todas las actividades del usuario autenticado.
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<ActivityResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll(CancellationToken cancellationToken)
    {
        var userId = GetUserId();
        var activities = await _sender.Send(new GetAllActivitiesQuery(userId), cancellationToken);
        return Ok(activities);
    }

    /// <summary>
    /// Obtiene una actividad específica por su ID.
    /// </summary>
    /// <param name="id">ID de la actividad.</param>
    /// <param name="ct">Token de cancelación.</param>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(ActivityResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(Guid id, CancellationToken ct)
    {
        var userId = GetUserId();
        var result = await _sender.Send(new GetActivityByIdQuery(id, userId), ct);
        return result.ToActionResult();
    }

    /// <summary>
    /// Crea una nueva actividad para el usuario autenticado.
    /// </summary>
    /// <param name="command">Nombre, categoría y opciones.</param>
    /// <param name="ct">Token de cancelación.</param>
    [HttpPost]
    [ProducesResponseType(typeof(ActivityResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create([FromBody] CreateActivityCommand command, CancellationToken ct)
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
    /// Actualiza una actividad (solo dueño).
    /// </summary>
    /// <param name="id">ID de la actividad.</param>
    /// <param name="command">Nuevos datos.</param>
    /// <param name="ct">Token de cancelación.</param>
    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(ActivityResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateActivityCommand command, CancellationToken ct)
    {
        var userId = GetUserId();
        var result = await _sender.Send(command with { Id = id, UserId = userId }, ct);
        return result.ToActionResult();
    }

    /// <summary>
    /// Elimina una actividad (solo dueño).
    /// </summary>
    /// <param name="id">ID de la actividad.</param>
    /// <param name="ct">Token de cancelación.</param>
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
    {
        var userId = GetUserId();
        var result = await _sender.Send(new DeleteActivityCommand(id, userId), ct);
        return result.ToActionResult();
    }
}

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MediatR;
using THtracker.API.Extensions;
using THtracker.Application.Common;
using THtracker.Application.Interfaces;
using THtracker.Application.Features.ActivityValueDefinitions;
using THtracker.Application.Features.ActivityValueDefinitions.Commands.CreateValueDefinition;
using THtracker.Application.Features.ActivityValueDefinitions.Commands.UpdateValueDefinition;
using THtracker.Application.Features.ActivityValueDefinitions.Commands.DeleteValueDefinition;
using THtracker.Application.Features.ActivityValueDefinitions.Queries.GetAllValueDefinitions;
using THtracker.Application.Features.ActivityValueDefinitions.Queries.GetValueDefinitionById;

namespace THtracker.API.Controllers.v1;

/// <summary>
/// Definiciones de valores medibles por actividad.
/// </summary>
[Authorize]
[ApiController]
[Route("activities/{activityId:guid}/definitions")]
public sealed class ActivityValueDefinitionsController : AuthorizedControllerBase
{
    private readonly ISender _sender;

    public ActivityValueDefinitionsController(ICurrentUserService currentUser, ISender sender) : base(currentUser)
    {
        _sender = sender;
    }

    /// <summary>
    /// Lista las definiciones de valores de una actividad.
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(PaginatedResponse<ActivityValueDefinitionResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll(
        Guid activityId,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = Pagination.DefaultPageSize,
        CancellationToken ct = default)
    {
        var userId = GetUserId();
        var result = await _sender.Send(new GetAllValueDefinitionsQuery(activityId, userId, pageNumber, pageSize), ct);
        return result.ToActionResult();
    }

    /// <summary>
    /// Obtiene una definición de valor por ID.
    /// </summary>
    [HttpGet("{definitionId:guid}")]
    [ProducesResponseType(typeof(ActivityValueDefinitionResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(Guid activityId, Guid definitionId, CancellationToken ct)
    {
        var userId = GetUserId();
        var result = await _sender.Send(new GetValueDefinitionByIdQuery(activityId, definitionId, userId), ct);
        return result.ToActionResult();
    }

    /// <summary>
    /// Crea una definición de valor para la actividad.
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(ActivityValueDefinitionResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create(
        Guid activityId,
        [FromBody] CreateValueDefinitionCommand command,
        CancellationToken ct)
    {
        var userId = GetUserId();
        var result = await _sender.Send(command with { ActivityId = activityId, UserId = userId }, ct);
        
        if (result.IsSuccess)
        {
            return CreatedAtAction(nameof(GetById), new { activityId, definitionId = result.Value.Id }, result.Value);
        }

        return result.ToActionResult();
    }

    /// <summary>
    /// Actualiza una definición de valor.
    /// </summary>
    [HttpPut("{definitionId:guid}")]
    [ProducesResponseType(typeof(ActivityValueDefinitionResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(
        Guid activityId, 
        Guid definitionId, 
        [FromBody] UpdateValueDefinitionCommand command, 
        CancellationToken ct)
    {
        var userId = GetUserId();
        var result = await _sender.Send(command with { ActivityId = activityId, DefinitionId = definitionId, UserId = userId }, ct);
        return result.ToActionResult();
    }

    /// <summary>
    /// Elimina una definición de valor.
    /// </summary>
    [HttpDelete("{definitionId:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(Guid activityId, Guid definitionId, CancellationToken ct)
    {
        var userId = GetUserId();
        var result = await _sender.Send(new DeleteValueDefinitionCommand(activityId, definitionId, userId), ct);
        
        if (result.IsSuccess)
        {
            return NoContent();
        }

        return result.ToActionResult();
    }
}
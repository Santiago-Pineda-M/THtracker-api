using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using THtracker.API.DTOs;
using THtracker.Application.DTOs.ActivityValueDefinitions;
using THtracker.Application.UseCases.ActivityValueDefinitions;

namespace THtracker.API.Controllers;

/// <summary>
/// Definiciones de valores medibles por actividad (ej. cantidad, duración, notas). REST: GET colección, GET por id, POST con Location al recurso creado.
/// </summary>
[Authorize]
[ApiController]
[Route("api/v1/activities/{activityId:guid}/definitions")]
public class ActivityValueDefinitionsController : AuthorizedControllerBase
{
    private readonly GetValueDefinitionsUseCase _getDefinitions;
    private readonly GetValueDefinitionByIdUseCase _getDefinitionById;
    private readonly CreateValueDefinitionUseCase _createDefinition;

    public ActivityValueDefinitionsController(
        GetValueDefinitionsUseCase getDefinitions,
        GetValueDefinitionByIdUseCase getDefinitionById,
        CreateValueDefinitionUseCase createDefinition)
    {
        _getDefinitions = getDefinitions;
        _getDefinitionById = getDefinitionById;
        _createDefinition = createDefinition;
    }

    /// <summary>
    /// Lista las definiciones de valores de una actividad.
    /// </summary>
    /// <param name="activityId">ID de la actividad.</param>
    /// <param name="cancellationToken">Token de cancelación.</param>
    /// <returns>Lista de definiciones (nombre, tipo, unidad, etc.).</returns>
    /// <response code="200">Lista de definiciones.</response>
    /// <response code="400">Error de negocio.</response>
    /// <response code="401">No autenticado.</response>
    /// <response code="403">No es dueño de la actividad.</response>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<ActivityValueDefinitionResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetAll(Guid activityId, CancellationToken cancellationToken)
    {
        try
        {
            var userId = GetUserId();
            var definitions = await _getDefinitions.ExecuteAsync(userId, activityId, cancellationToken);
            return Ok(definitions);
        }
        catch (Exception ex)
        {
            return BadRequest(new ApiErrorResponse(ex.Message));
        }
    }

    /// <summary>
    /// Obtiene una definición de valor por ID (solo si pertenece a la actividad del usuario).
    /// </summary>
    /// <param name="activityId">ID de la actividad.</param>
    /// <param name="definitionId">ID de la definición.</param>
    /// <param name="cancellationToken">Token de cancelación.</param>
    /// <returns>Definición (nombre, tipo, unidad, mín/máx).</returns>
    /// <response code="200">Definición.</response>
    /// <response code="401">No autenticado.</response>
    /// <response code="403">No es dueño de la actividad.</response>
    /// <response code="404">Definición no encontrada.</response>
    [HttpGet("{definitionId:guid}")]
    [ProducesResponseType(typeof(ActivityValueDefinitionResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(Guid activityId, Guid definitionId, CancellationToken cancellationToken)
    {
        var userId = GetUserId();
        var definition = await _getDefinitionById.ExecuteAsync(userId, activityId, definitionId, cancellationToken);
        return definition == null ? NotFound() : Ok(definition);
    }

    /// <summary>
    /// Crea una definición de valor para la actividad (ej. "Cantidad" numérico, "Notas" texto).
    /// </summary>
    /// <param name="activityId">ID de la actividad.</param>
    /// <param name="request">Nombre, tipo de valor, obligatorio, unidad, mín/máx.</param>
    /// <param name="cancellationToken">Token de cancelación.</param>
    /// <returns>Definición creada y cabecera Location a GET por ID.</returns>
    /// <response code="201">Definición creada.</response>
    /// <response code="400">Datos inválidos.</response>
    /// <response code="401">No autenticado.</response>
    /// <response code="403">No es dueño de la actividad.</response>
    [HttpPost]
    [ProducesResponseType(typeof(ActivityValueDefinitionResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> Create(
        Guid activityId,
        [FromBody] CreateValueDefinitionRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            var userId = GetUserId();
            var definition = await _createDefinition.ExecuteAsync(userId, activityId, request, cancellationToken);
            return CreatedAtAction(nameof(GetById), new { activityId, definitionId = definition.Id }, definition);
        }
        catch (Exception ex)
        {
            return BadRequest(new ApiErrorResponse(ex.Message));
        }
    }
}

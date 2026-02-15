using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using THtracker.API.Extensions;
using THtracker.Application.DTOs.ActivityValueDefinitions;
using THtracker.Application.UseCases.ActivityValueDefinitions;

namespace THtracker.API.Controllers.v1;

/// <summary>
/// Definiciones de valores medibles por actividad.
/// </summary>
[Authorize]
[ApiController]
[Route("activities/{activityId:guid}/definitions")]
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
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<ActivityValueDefinitionResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetAll(Guid activityId, CancellationToken cancellationToken)
    {
        var userId = GetUserId();
        var result = await _getDefinitions.ExecuteAsync(userId, activityId, cancellationToken);
        return result.ToActionResult();
    }

    /// <summary>
    /// Obtiene una definición de valor por ID.
    /// </summary>
    /// <param name="activityId">ID de la actividad.</param>
    /// <param name="definitionId">ID de la definición.</param>
    /// <param name="cancellationToken">Token de cancelación.</param>
    [HttpGet("{definitionId:guid}")]
    [ProducesResponseType(typeof(ActivityValueDefinitionResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(Guid activityId, Guid definitionId, CancellationToken cancellationToken)
    {
        var userId = GetUserId();
        var result = await _getDefinitionById.ExecuteAsync(userId, activityId, definitionId, cancellationToken);
        return result.ToActionResult();
    }

    /// <summary>
    /// Crea una definición de valor para la actividad.
    /// </summary>
    /// <param name="activityId">ID de la actividad.</param>
    /// <param name="request">Nombre, tipo de valor, obligatorio, unidad, mín/máx.</param>
    /// <param name="cancellationToken">Token de cancelación.</param>
    [HttpPost]
    [ProducesResponseType(typeof(ActivityValueDefinitionResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> Create(
        Guid activityId,
        [FromBody] CreateValueDefinitionRequest request,
        CancellationToken cancellationToken)
    {
        var userId = GetUserId();
        var result = await _createDefinition.ExecuteAsync(userId, activityId, request, cancellationToken);
        
        if (result.IsSuccess)
        {
            return CreatedAtAction(nameof(GetById), new { activityId, definitionId = result.Value.Id }, result.Value);
        }

        return result.ToActionResult();
    }
}

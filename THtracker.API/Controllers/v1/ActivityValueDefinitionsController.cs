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
    private readonly UpdateValueDefinitionUseCase _updateDefinition;
    private readonly DeleteValueDefinitionUseCase _deleteDefinition;

    public ActivityValueDefinitionsController(
        GetValueDefinitionsUseCase getDefinitions,
        GetValueDefinitionByIdUseCase getDefinitionById,
        CreateValueDefinitionUseCase createDefinition,
        UpdateValueDefinitionUseCase updateDefinition,
        DeleteValueDefinitionUseCase deleteDefinition)
    {
        _getDefinitions = getDefinitions;
        _getDefinitionById = getDefinitionById;
        _createDefinition = createDefinition;
        _updateDefinition = updateDefinition;
        _deleteDefinition = deleteDefinition;
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

    /// <summary>
    /// Actualiza una definición de valor.
    /// </summary>
    /// <param name="activityId">ID de la actividad.</param>
    /// <param name="definitionId">ID de la definición.</param>
    /// <param name="request">Datos a actualizar.</param>
    /// <param name="cancellationToken">Token de cancelación.</param>
    [HttpPut("{definitionId:guid}")]
    [ProducesResponseType(typeof(ActivityValueDefinitionResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(
        Guid activityId,
        Guid definitionId,
        [FromBody] UpdateValueDefinitionRequest request,
        CancellationToken cancellationToken)
    {
        var userId = GetUserId();
        var result = await _updateDefinition.ExecuteAsync(userId, activityId, definitionId, request, cancellationToken);
        return result.ToActionResult();
    }

    /// <summary>
    /// Elimina una definición de valor.
    /// </summary>
    /// <param name="activityId">ID de la actividad.</param>
    /// <param name="definitionId">ID de la definición.</param>
    /// <param name="cancellationToken">Token de cancellationToken.</param>
    [HttpDelete("{definitionId:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(
        Guid activityId,
        Guid definitionId,
        CancellationToken cancellationToken)
    {
        var userId = GetUserId();
        var result = await _deleteDefinition.ExecuteAsync(userId, activityId, definitionId, cancellationToken);
        return result.ToActionResult();
    }
}
namespace THtracker.API.Controllers.v1;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using THtracker.API.Extensions;
using THtracker.Application.DTOs.Tasks;
using THtracker.Application.UseCases.Tasks;

[Authorize]
[ApiController]
[Route("tasks")]
public class TasksController : AuthorizedControllerBase
{
    private readonly GetAllTasksByTaskListUseCase getAllTasksByTaskList;
    private readonly GetTaskByIdUseCase getTaskById;
    private readonly CreateTaskItemUseCase createTask;
    private readonly UpdateTaskItemUseCase updateTask;
    private readonly ToggleTaskCompletionUseCase toggleTaskCompletion;
    private readonly DeleteTaskItemUseCase deleteTask;

    public TasksController(
        GetAllTasksByTaskListUseCase getAllTasksByTaskList,
        GetTaskByIdUseCase getTaskById,
        CreateTaskItemUseCase createTask,
        UpdateTaskItemUseCase updateTask,
        ToggleTaskCompletionUseCase toggleTaskCompletion,
        DeleteTaskItemUseCase deleteTask
    )
    {
        this.getAllTasksByTaskList = getAllTasksByTaskList;
        this.getTaskById = getTaskById;
        this.createTask = createTask;
        this.updateTask = updateTask;
        this.toggleTaskCompletion = toggleTaskCompletion;
        this.deleteTask = deleteTask;
    }

    [HttpGet("by-task-list/{taskListId:guid}")]
    [ProducesResponseType(typeof(IEnumerable<TaskItemResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetAllByTaskList(
        Guid taskListId,
        CancellationToken cancellationToken
    )
    {
        var userId = GetUserId();
        var result = this
            .getAllTasksByTaskList.ExecuteAsync(userId, taskListId, cancellationToken)
            .Result;
        return result.ToActionResult();
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(TaskItemResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
    {
        var userId = GetUserId();
        var result = this.getTaskById.ExecuteAsync(userId, id, cancellationToken).Result;
        return result.ToActionResult();
    }

    [HttpPost]
    [ProducesResponseType(typeof(TaskItemResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> Create(
        [FromBody] CreateTaskItemRequest request,
        CancellationToken cancellationToken
    )
    {
        var userId = GetUserId();
        var result = this.createTask.ExecuteAsync(userId, request, cancellationToken).Result;

        if (result.IsSuccess)
        {
            return CreatedAtAction(
                nameof(this.GetById),
                new { id = result.Value.Id },
                result.Value
            );
        }

        return result.ToActionResult();
    }

    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(TaskItemResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(
        Guid id,
        [FromBody] UpdateTaskItemRequest request,
        CancellationToken cancellationToken
    )
    {
        var userId = GetUserId();
        var result = this.updateTask.ExecuteAsync(userId, id, request, cancellationToken).Result;
        return result.ToActionResult();
    }

    [HttpPatch("{id:guid}/toggle")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ToggleCompletion(Guid id, CancellationToken cancellationToken)
    {
        var userId = GetUserId();
        var result = this.toggleTaskCompletion.ExecuteAsync(userId, id, cancellationToken).Result;

        if (result.IsSuccess)
        {
            return NoContent();
        }

        return result.ToActionResult();
    }

    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        var userId = GetUserId();
        var result = this.deleteTask.ExecuteAsync(userId, id, cancellationToken).Result;

        if (result.IsSuccess)
        {
            return NoContent();
        }

        return result.ToActionResult();
    }
}

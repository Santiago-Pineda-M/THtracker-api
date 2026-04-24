namespace THtracker.API.Controllers.v1;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using THtracker.API.Extensions;
using THtracker.Application.DTOs.TaskLists;
using THtracker.Application.UseCases.TaskLists;

[Authorize]
[ApiController]
[Route("task-lists")]
public class TaskListsController : AuthorizedControllerBase
{
    private readonly GetAllTaskListsUseCase getAllTaskLists;
    private readonly GetTaskListByIdUseCase getTaskListById;
    private readonly CreateTaskListUseCase createTaskList;
    private readonly UpdateTaskListUseCase updateTaskList;
    private readonly DeleteTaskListUseCase deleteTaskList;

    public TaskListsController(
        GetAllTaskListsUseCase getAllTaskLists,
        GetTaskListByIdUseCase getTaskListById,
        CreateTaskListUseCase createTaskList,
        UpdateTaskListUseCase updateTaskList,
        DeleteTaskListUseCase deleteTaskList
    )
    {
        this.getAllTaskLists = getAllTaskLists;
        this.getTaskListById = getTaskListById;
        this.createTaskList = createTaskList;
        this.updateTaskList = updateTaskList;
        this.deleteTaskList = deleteTaskList;
    }

    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<TaskListResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll(CancellationToken cancellationToken)
    {
        var userId = GetUserId();
        var taskLists = await this.getAllTaskLists.ExecuteAsync(userId, cancellationToken);
        return Ok(taskLists);
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(TaskListResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
    {
        var userId = GetUserId();
        var result = await this.getTaskListById.ExecuteAsync(userId, id, cancellationToken);
        return result.ToActionResult();
    }

    [HttpPost]
    [ProducesResponseType(typeof(TaskListResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create(
        [FromBody] CreateTaskListRequest request,
        CancellationToken cancellationToken
    )
    {
        var userId = GetUserId();
        var result = await this.createTaskList.ExecuteAsync(userId, request, cancellationToken);

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
    [ProducesResponseType(typeof(TaskListResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(
        Guid id,
        [FromBody] UpdateTaskListRequest request,
        CancellationToken cancellationToken
    )
    {
        var userId = GetUserId();
        var result = await this
            .updateTaskList.ExecuteAsync(userId, id, request, cancellationToken);
        return result.ToActionResult();
    }

    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        var userId = GetUserId();
        var result = await this.deleteTaskList.ExecuteAsync(userId, id, cancellationToken);

        if (result.IsSuccess)
        {
            return NoContent();
        }

        return result.ToActionResult();
    }
}

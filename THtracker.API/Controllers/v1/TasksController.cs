using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MediatR;
using THtracker.API.Extensions;
using THtracker.Application.Features.Tasks;
using THtracker.Application.Features.Tasks.Commands.CreateTask;
using THtracker.Application.Features.Tasks.Commands.UpdateTask;
using THtracker.Application.Features.Tasks.Commands.DeleteTask;
using THtracker.Application.Features.Tasks.Commands.ToggleTaskCompletion;
using THtracker.Application.Features.Tasks.Queries.GetTaskById;
using THtracker.Application.Features.Tasks.Queries.GetAllTasksByTaskList;

namespace THtracker.API.Controllers.v1;

[Authorize]
[ApiController]
[Route("tasks")]
public sealed class TasksController : AuthorizedControllerBase
{
    private readonly ISender _sender;

    public TasksController(ISender sender)
    {
        _sender = sender;
    }

    [HttpGet("by-task-list/{taskListId:guid}")]
    [ProducesResponseType(typeof(IEnumerable<TaskResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetAllByTaskList(Guid taskListId, CancellationToken ct)
    {
        var userId = GetUserId();
        var result = await _sender.Send(new GetAllTasksByTaskListQuery(taskListId, userId), ct);
        return result.ToActionResult();
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(TaskResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(Guid id, CancellationToken ct)
    {
        var userId = GetUserId();
        var result = await _sender.Send(new GetTaskByIdQuery(id, userId), ct);
        return result.ToActionResult();
    }

    [HttpPost]
    [ProducesResponseType(typeof(TaskResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create([FromBody] CreateTaskCommand command, CancellationToken ct)
    {
        var userId = GetUserId();
        var result = await _sender.Send(command with { UserId = userId }, ct);

        if (result.IsSuccess)
        {
            return CreatedAtAction(nameof(GetById), new { id = result.Value.Id }, result.Value);
        }

        return result.ToActionResult();
    }

    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(TaskResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateTaskCommand command, CancellationToken ct)
    {
        var userId = GetUserId();
        var result = await _sender.Send(command with { Id = id, UserId = userId }, ct);
        return result.ToActionResult();
    }

    [HttpPatch("{id:guid}/toggle")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ToggleCompletion(Guid id, CancellationToken ct)
    {
        var userId = GetUserId();
        var result = await _sender.Send(new ToggleTaskCompletionCommand(id, userId), ct);
        
        if (result.IsSuccess)
        {
            return NoContent();
        }

        return result.ToActionResult();
    }

    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
    {
        var userId = GetUserId();
        var result = await _sender.Send(new DeleteTaskCommand(id, userId), ct);
        
        if (result.IsSuccess)
        {
            return NoContent();
        }

        return result.ToActionResult();
    }
}

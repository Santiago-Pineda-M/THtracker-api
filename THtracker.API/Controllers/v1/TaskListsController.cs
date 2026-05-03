using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MediatR;
using THtracker.API.Extensions;
using THtracker.Application.Features.TaskLists;
using THtracker.Application.Features.TaskLists.Commands.CreateTaskList;
using THtracker.Application.Features.TaskLists.Commands.UpdateTaskList;
using THtracker.Application.Features.TaskLists.Commands.DeleteTaskList;
using THtracker.Application.Features.TaskLists.Queries.GetAllTaskLists;
using THtracker.Application.Features.TaskLists.Queries.GetTaskListById;

namespace THtracker.API.Controllers.v1;

[Authorize]
[ApiController]
[Route("task-lists")]
public sealed class TaskListsController : AuthorizedControllerBase
{
    private readonly ISender _sender;

    public TaskListsController(ISender sender)
    {
        _sender = sender;
    }

    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<TaskListResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll(CancellationToken ct)
    {
        var userId = GetUserId();
        var result = await _sender.Send(new GetAllTaskListsQuery(userId), ct);
        return result.ToActionResult();
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(TaskListResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(Guid id, CancellationToken ct)
    {
        var userId = GetUserId();
        var result = await _sender.Send(new GetTaskListByIdQuery(id, userId), ct);
        return result.ToActionResult();
    }

    [HttpPost]
    [ProducesResponseType(typeof(TaskListResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create([FromBody] CreateTaskListCommand command, CancellationToken ct)
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
    [ProducesResponseType(typeof(TaskListResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateTaskListCommand command, CancellationToken ct)
    {
        var userId = GetUserId();
        var result = await _sender.Send(command with { Id = id, UserId = userId }, ct);
        return result.ToActionResult();
    }

    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
    {
        var userId = GetUserId();
        var result = await _sender.Send(new DeleteTaskListCommand(id, userId), ct);

        if (result.IsSuccess)
        {
            return NoContent();
        }

        return result.ToActionResult();
    }
}

namespace THtracker.Application.UseCases.TaskLists;

using THtracker.Application.DTOs.TaskLists;
using THtracker.Domain.Interfaces;

public class GetAllTaskListsUseCase
{
    private readonly ITaskListRepository taskListRepository;

    public GetAllTaskListsUseCase(ITaskListRepository taskListRepository)
    {
        this.taskListRepository = taskListRepository;
    }

    public async Task<IEnumerable<TaskListResponse>> ExecuteAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var taskLists = await this.taskListRepository.GetAllByUserAsync(userId, cancellationToken);
        return taskLists.Select(t => new TaskListResponse(t.Id, t.UserId, t.Name, t.Color, t.CreatedAt, t.UpdatedAt));
    }
}

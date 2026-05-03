using MediatR;
using THtracker.Domain.Common;
using THtracker.Domain.Entities;
using THtracker.Domain.Interfaces;

namespace THtracker.Application.Features.TaskLists.Commands.CreateTaskList;

public sealed class CreateTaskListCommandHandler : IRequestHandler<CreateTaskListCommand, Result<TaskListResponse>>
{
    private readonly ITaskListRepository _taskListRepository;
    private readonly IUnitOfWork _unitOfWork;

    public CreateTaskListCommandHandler(ITaskListRepository taskListRepository, IUnitOfWork unitOfWork)
    {
        _taskListRepository = taskListRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<TaskListResponse>> Handle(CreateTaskListCommand request, CancellationToken cancellationToken)
    {
        var taskList = new TaskList(request.UserId, request.Name, request.Color);

        await _taskListRepository.AddAsync(taskList, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return new TaskListResponse(
            taskList.Id,
            taskList.UserId,
            taskList.Name,
            taskList.Color,
            taskList.CreatedAt,
            taskList.UpdatedAt
        );
    }
}

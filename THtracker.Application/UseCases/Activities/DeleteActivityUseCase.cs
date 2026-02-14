using THtracker.Domain.Interfaces;

namespace THtracker.Application.UseCases.Activities;

public class DeleteActivityUseCase
{
    private readonly IActivityRepository _activityRepository;

    public DeleteActivityUseCase(IActivityRepository activityRepository)
    {
        _activityRepository = activityRepository;
    }

    public async Task<bool> ExecuteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _activityRepository.DeleteAsync(id, cancellationToken);
    }
}

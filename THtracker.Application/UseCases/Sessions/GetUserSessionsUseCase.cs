using THtracker.Application.DTOs.Sessions;
using THtracker.Domain.Interfaces;

namespace THtracker.Application.UseCases.Sessions;

public class GetUserSessionsUseCase
{
    private readonly IUserSessionRepository _sessionRepository;

    public GetUserSessionsUseCase(IUserSessionRepository sessionRepository)
    {
        _sessionRepository = sessionRepository;
    }

    public async Task<IEnumerable<UserSessionResponse>> ExecuteAsync(
        Guid userId,
        CancellationToken cancellationToken = default
    )
    {
        var sessions = await _sessionRepository.GetActiveByUserAsync(userId, cancellationToken);
        return sessions.Select(s => new UserSessionResponse(
            s.Id,
            s.DeviceInfo,
            s.IpAddress,
            s.Location,
            s.CreatedAt,
            s.ExpiresAt,
            s.IsActive
        ));
    }
}

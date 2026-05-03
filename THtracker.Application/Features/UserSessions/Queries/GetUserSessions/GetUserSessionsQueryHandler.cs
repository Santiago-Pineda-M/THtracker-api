using MediatR;
using THtracker.Domain.Common;
using THtracker.Domain.Interfaces;

namespace THtracker.Application.Features.UserSessions.Queries.GetUserSessions;

public sealed class GetUserSessionsQueryHandler : IRequestHandler<GetUserSessionsQuery, Result<IEnumerable<UserSessionResponse>>>
{
    private readonly IUserSessionRepository _sessionRepository;

    public GetUserSessionsQueryHandler(IUserSessionRepository sessionRepository)
    {
        _sessionRepository = sessionRepository;
    }

    public async Task<Result<IEnumerable<UserSessionResponse>>> Handle(GetUserSessionsQuery request, CancellationToken cancellationToken)
    {
        var sessions = await _sessionRepository.GetActiveByUserAsync(request.UserId, cancellationToken);
        
        var response = sessions.Select(s => new UserSessionResponse(
            s.Id,
            s.DeviceInfo,
            s.IpAddress,
            s.Location,
            s.CreatedAt,
            s.ExpiresAt,
            s.IsActive
        ));

        return Result.Success(response);
    }
}

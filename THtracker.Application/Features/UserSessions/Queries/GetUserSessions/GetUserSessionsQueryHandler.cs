using MediatR;
using THtracker.Application.Common;
using THtracker.Domain.Common;
using THtracker.Domain.Interfaces;

namespace THtracker.Application.Features.UserSessions.Queries.GetUserSessions;

public sealed class GetUserSessionsQueryHandler : IRequestHandler<GetUserSessionsQuery, Result<PaginatedResponse<UserSessionResponse>>>
{
    private readonly IUserSessionRepository _sessionRepository;

    public GetUserSessionsQueryHandler(IUserSessionRepository sessionRepository)
    {
        _sessionRepository = sessionRepository;
    }

    public async Task<Result<PaginatedResponse<UserSessionResponse>>> Handle(
        GetUserSessionsQuery request,
        CancellationToken cancellationToken)
    {
        var (pageNumber, pageSize) = Pagination.Normalize(request.PageNumber, request.PageSize);
        var page = await _sessionRepository.GetActivePageByUserAsync(
            request.UserId,
            pageNumber,
            pageSize,
            cancellationToken);

        var items = page.Items
            .Select(s => new UserSessionResponse(
                s.Id,
                s.DeviceInfo,
                s.IpAddress,
                s.Location,
                s.CreatedAt,
                s.ExpiresAt,
                s.IsActive))
            .ToList();

        return Result.Success(new PaginatedResponse<UserSessionResponse>(
            items,
            page.TotalCount,
            pageNumber,
            pageSize));
    }
}

using MediatR;
using THtracker.Application.Common;
using THtracker.Application.Features.Users.Queries.GetUserById;
using THtracker.Domain.Common;
using THtracker.Domain.Interfaces;

namespace THtracker.Application.Features.Users.Queries.GetAllUsers;

public sealed class GetAllUsersQueryHandler : IRequestHandler<GetAllUsersQuery, Result<PaginatedResponse<UserResponse>>>
{
    private readonly IUserRepository _userRepository;

    public GetAllUsersQueryHandler(IUserRepository userRepository)
    {
        _userRepository = userRepository;
    }

    public async Task<Result<PaginatedResponse<UserResponse>>> Handle(
        GetAllUsersQuery request,
        CancellationToken cancellationToken)
    {
        var (pageNumber, pageSize) = Pagination.Normalize(request.PageNumber, request.PageSize);
        var page = await _userRepository.GetPageAsync(pageNumber, pageSize, cancellationToken);

        var items = page.Items
            .Select(u => new UserResponse(u.Id, u.Name, u.Email))
            .ToList();

        return Result.Success(new PaginatedResponse<UserResponse>(
            items,
            page.TotalCount,
            pageNumber,
            pageSize));
    }
}

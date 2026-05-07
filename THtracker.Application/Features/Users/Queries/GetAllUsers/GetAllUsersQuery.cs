using MediatR;
using THtracker.Application.Common;
using THtracker.Application.Features.Users.Queries.GetUserById;
using THtracker.Domain.Common;

namespace THtracker.Application.Features.Users.Queries.GetAllUsers;

public sealed record GetAllUsersQuery(
    int PageNumber = 1,
    int PageSize = Pagination.DefaultPageSize) : IRequest<Result<PaginatedResponse<UserResponse>>>;

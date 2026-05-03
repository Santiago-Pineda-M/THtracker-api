using MediatR;
using THtracker.Domain.Common;

namespace THtracker.Application.Features.Users.Queries.GetUserById;

public sealed record GetUserByIdQuery(Guid Id) : IRequest<Result<UserResponse>>;

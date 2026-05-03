using MediatR;
using THtracker.Domain.Common;
using THtracker.Application.Features.Users.Queries.GetUserById;

namespace THtracker.Application.Features.Users.Commands.UpdateUser;

public sealed record UpdateUserCommand(
    Guid Id,
    string Name,
    string Email) : IRequest<Result<UserResponse>>;

using MediatR;
using THtracker.Domain.Common;
using THtracker.Application.Features.Users.Queries.GetUserById;

namespace THtracker.Application.Features.Users.Commands.CreateUser;

public sealed record CreateUserCommand(
    string Name, 
    string Email, 
    string Password) : IRequest<Result<UserResponse>>;

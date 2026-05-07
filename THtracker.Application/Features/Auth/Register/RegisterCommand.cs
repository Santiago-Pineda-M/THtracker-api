using MediatR;
using THtracker.Application.Features.Users.Queries.GetUserById;
using THtracker.Domain.Common;

namespace THtracker.Application.Features.Auth.Register;

public sealed record RegisterCommand(
    string Name,
    string Email,
    string Password) : IRequest<Result<UserResponse>>;

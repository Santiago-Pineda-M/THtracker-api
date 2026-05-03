using MediatR;
using THtracker.Domain.Common;

namespace THtracker.Application.Features.Users.Commands.DeleteUser;

public sealed record DeleteUserCommand(Guid Id) : IRequest<Result<Unit>>;

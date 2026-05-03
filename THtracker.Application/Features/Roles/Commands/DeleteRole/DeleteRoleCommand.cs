using MediatR;
using THtracker.Domain.Common;

namespace THtracker.Application.Features.Roles.Commands.DeleteRole;

public sealed record DeleteRoleCommand(Guid Id) : IRequest<Result>;

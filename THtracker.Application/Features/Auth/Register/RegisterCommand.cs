using MediatR;
using THtracker.Domain.Common;

namespace THtracker.Application.Features.Auth.Register;

public sealed record RegisterCommand(
    string Name, 
    string Email, 
    string Password) : IRequest<Result<Guid>>;

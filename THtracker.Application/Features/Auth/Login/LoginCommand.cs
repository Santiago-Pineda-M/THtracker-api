using MediatR;
using THtracker.Domain.Common;

namespace THtracker.Application.Features.Auth.Login;

public sealed record LoginCommand(
    string Email, 
    string Password, 
    string DeviceInfo, 
    string IpAddress) : IRequest<Result<TokenResponse>>;

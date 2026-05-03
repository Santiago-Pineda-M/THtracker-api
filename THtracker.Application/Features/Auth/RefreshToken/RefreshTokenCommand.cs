using MediatR;
using THtracker.Domain.Common;

namespace THtracker.Application.Features.Auth.RefreshToken;

public sealed record RefreshTokenCommand(
    string RefreshToken,
    string IpAddress,
    string DeviceInfo) : IRequest<Result<TokenResponse>>;

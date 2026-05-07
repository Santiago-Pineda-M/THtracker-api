using MediatR;
using THtracker.Domain.Common;

namespace THtracker.Application.Features.Auth.RefreshToken;

public sealed record RefreshTokenCommand(
    string RefreshToken,
    string DeviceInfo) : IRequest<Result<TokenResponse>>;

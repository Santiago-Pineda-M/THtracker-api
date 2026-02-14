using THtracker.Domain.Entities;

namespace THtracker.Application.Interfaces;

public interface IJwtProvider
{
    string GenerateAccessToken(User user);
    RefreshToken GenerateRefreshToken(User user, string ipAddress, string deviceInfo);
}

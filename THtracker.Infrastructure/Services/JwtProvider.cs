using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using THtracker.Application.Interfaces;
using THtracker.Domain.Entities;
using THtracker.Infrastructure.Options;

namespace THtracker.Infrastructure.Services;

public sealed class JwtProvider(IOptions<JwtOptions> jwtOptions) : IJwtProvider
{
    private readonly JwtOptions _jwt = jwtOptions.Value;

    public string GenerateAccessToken(User user, Guid sessionId)
    {
        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new(JwtRegisteredClaimNames.Email, user.Email),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new("sid", sessionId.ToString())
        };

        foreach (var role in user.Roles)
        {
            claims.Add(new Claim(ClaimTypes.Role, role.Name));
            foreach (var permission in role.Permissions)
            {
                claims.Add(new Claim("permission", permission.Name));
            }
        }

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwt.SecretKey));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: _jwt.Issuer,
            audience: _jwt.Audience,
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(_jwt.AccessTokenExpirationMinutes),
            signingCredentials: creds
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    public RefreshToken GenerateRefreshToken(User user, string ipAddress, string deviceInfo)
    {
        var randomNumber = new byte[32];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(randomNumber);
        string token = Convert.ToBase64String(randomNumber);

        var expiry = DateTime.UtcNow.AddDays(_jwt.RefreshTokenExpirationDays);

        return new RefreshToken(token, expiry, ipAddress, deviceInfo, user.Id);
    }
}

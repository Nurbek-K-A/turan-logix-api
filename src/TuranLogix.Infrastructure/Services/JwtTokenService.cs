using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using TuranLogix.Application.Common.Interfaces;
using TuranLogix.Domain.Enums;

namespace TuranLogix.Infrastructure.Services;

/// <summary>
/// Генерирует подписанные JWT Bearer токены на основе конфигурации Jwt:*
/// </summary>
public class JwtTokenService : IJwtTokenService
{
    private readonly IConfiguration _configuration;

    /// <param name="configuration">Конфигурация приложения (секции Jwt:Key, Jwt:Issuer, Jwt:Audience)</param>
    public JwtTokenService(IConfiguration configuration) => _configuration = configuration;

    /// <inheritdoc/>
    /// <remarks>Токен действителен 7 дней, подписан HMAC-SHA256</remarks>
    public string GenerateToken(int userId, string email, UserRole role)
    {
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]!));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, userId.ToString()),
            new Claim(ClaimTypes.Email, email),
            new Claim(ClaimTypes.Role, role.ToString()),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

        var token = new JwtSecurityToken(
            issuer: _configuration["Jwt:Issuer"],
            audience: _configuration["Jwt:Audience"],
            claims: claims,
            expires: DateTime.UtcNow.AddDays(7),
            signingCredentials: credentials);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}

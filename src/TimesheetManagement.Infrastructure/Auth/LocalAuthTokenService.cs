using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using TimesheetManagement.Application.Auth;
using TimesheetManagement.Domain.Entities;

namespace TimesheetManagement.Infrastructure.Auth;

public class LocalAuthTokenService(IOptions<LocalAuthOptions> options) : ILocalAuthTokenService
{
    public string IssueToken(User user)
    {
        var opts = options.Value;
        if (string.IsNullOrWhiteSpace(opts.SigningKey))
        {
            throw new InvalidOperationException(
                "Auth:Local:SigningKey is not configured. Set it via 'dotnet user-secrets set Auth:Local:SigningKey <value>' in development.");
        }

        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Name, user.FullName),
            new Claim(ClaimTypes.Email, user.Email),
            new Claim(ClaimTypes.Role, user.Role.ToString())
        };

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(opts.SigningKey));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: opts.Issuer,
            audience: opts.Audience,
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(opts.TokenLifetimeMinutes),
            signingCredentials: credentials);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}

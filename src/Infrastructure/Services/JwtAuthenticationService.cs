using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using PathfinderCampaignManager.Application.Abstractions;
using PathfinderCampaignManager.Domain.Entities;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace PathfinderCampaignManager.Infrastructure.Services;

public class JwtAuthenticationService : IJwtAuthenticationService
{
    private readonly IConfiguration _configuration;
    private readonly string _secretKey;
    private readonly string _issuer;
    private readonly string _audience;

    public JwtAuthenticationService(IConfiguration configuration)
    {
        _configuration = configuration;
        _secretKey = configuration["Jwt:SecretKey"] ?? "your-very-secure-secret-key-that-is-at-least-256-bits-long";
        _issuer = configuration["Jwt:Issuer"] ?? "PathfinderCampaignManager";
        _audience = configuration["Jwt:Audience"] ?? "PathfinderCampaignManager.Client";
    }

    public string GenerateToken(User user)
    {
        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Email, user.Email),
            new Claim(ClaimTypes.Name, user.Username),
            new Claim("display_name", user.DisplayName),
            new Claim(ClaimTypes.Role, user.Role.ToString()),
            new Claim("is_active", user.IsActive.ToString().ToLower()),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new Claim(JwtRegisteredClaimNames.Iat, DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString(), ClaimValueTypes.Integer64)
        };

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_secretKey));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: _issuer,
            audience: _audience,
            claims: claims,
            expires: DateTime.UtcNow.AddDays(7), // 7-day expiration
            signingCredentials: creds
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    public ClaimsPrincipal? ValidateToken(string token)
    {
        try
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.UTF8.GetBytes(_secretKey);

            tokenHandler.ValidateToken(token, new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(key),
                ValidateIssuer = true,
                ValidIssuer = _issuer,
                ValidateAudience = true,
                ValidAudience = _audience,
                ValidateLifetime = true,
                ClockSkew = TimeSpan.Zero
            }, out SecurityToken validatedToken);

            var jwtToken = (JwtSecurityToken)validatedToken;
            var userId = jwtToken.Claims.First(x => x.Type == ClaimTypes.NameIdentifier).Value;

            return new ClaimsPrincipal(new ClaimsIdentity(jwtToken.Claims, "jwt"));
        }
        catch
        {
            return null;
        }
    }

    public SymmetricSecurityKey GetSecretKey() => new(Encoding.UTF8.GetBytes(_secretKey));
}
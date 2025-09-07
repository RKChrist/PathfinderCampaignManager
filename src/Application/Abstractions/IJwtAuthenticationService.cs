using PathfinderCampaignManager.Domain.Entities;
using System.Security.Claims;

namespace PathfinderCampaignManager.Application.Abstractions;

public interface IJwtAuthenticationService
{
    string GenerateToken(User user);
    ClaimsPrincipal? ValidateToken(string token);
}
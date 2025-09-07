using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using PathfinderCampaignManager.Application.Abstractions;
using PathfinderCampaignManager.Application.Services;
using PathfinderCampaignManager.Domain.Entities;
using System.Security.Claims;
using System.Text.Json;
using System.ComponentModel.DataAnnotations;

namespace PathfinderCampaignManager.Presentation.Server.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly AuthenticationService _authService;
    private readonly IJwtAuthenticationService _jwtService;
    private readonly IConfiguration _configuration;
    private readonly HttpClient _httpClient;

    public AuthController(
        AuthenticationService authService,
        IJwtAuthenticationService jwtService,
        IConfiguration configuration,
        HttpClient httpClient)
    {
        _authService = authService;
        _jwtService = jwtService;
        _configuration = configuration;
        _httpClient = httpClient;
    }

    [HttpPost("discord-callback")]
    public async Task<IActionResult> DiscordCallback([FromBody] DiscordCallbackRequest request)
    {
        try
        {
            // Exchange the code for an access token
            var tokenResponse = await ExchangeCodeForToken(request.Code);
            if (tokenResponse == null)
                return BadRequest("Failed to exchange code for token");

            // Get user info from Discord
            var discordUser = await GetDiscordUserInfo(tokenResponse.AccessToken);
            if (discordUser == null)
                return BadRequest("Failed to get user information from Discord");

            // Authenticate with Discord
            var result = await _authService.LoginWithDiscordAsync(
                discordUser.Id,
                discordUser.Email ?? "",
                discordUser.Username,
                discordUser.GlobalName ?? discordUser.Username);

            if (!result.IsSuccess)
                return BadRequest(result.ErrorMessage);

            return Ok(new AuthResponse
            {
                Token = result.Token!,
                User = MapToUserInfo(result.User!)
            });
        }
        catch (Exception ex)
        {
            return BadRequest($"Authentication failed: {ex.Message}");
        }
    }

    [HttpGet("discord-login-url")]
    public IActionResult GetDiscordLoginUrl()
    {
        var clientId = _configuration["Discord:ClientId"];
        var redirectUri = _configuration["Discord:RedirectUri"] ?? "http://localhost:7082/auth/discord-callback";
        
        var discordLoginUrl = $"https://discord.com/api/oauth2/authorize?" +
            $"client_id={clientId}&" +
            $"redirect_uri={Uri.EscapeDataString(redirectUri)}&" +
            $"response_type=code&" +
            $"scope=identify%20email";

        return Ok(new { LoginUrl = discordLoginUrl });
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterRequest request)
    {
        try
        {
            var result = await _authService.RegisterAsync(
                request.Email, 
                request.Username, 
                request.Password, 
                request.DisplayName);

            if (!result.IsSuccess)
                return BadRequest(new { Error = result.ErrorMessage });

            return Ok(new AuthResponse
            {
                Token = result.Token!,
                User = MapToUserInfo(result.User!)
            });
        }
        catch (Exception ex)
        {
            return BadRequest($"Registration failed: {ex.Message}");
        }
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        try
        {
            var result = await _authService.LoginAsync(request.EmailOrUsername, request.Password);

            if (!result.IsSuccess)
                return BadRequest(new { Error = result.ErrorMessage });

            return Ok(new AuthResponse
            {
                Token = result.Token!,
                User = MapToUserInfo(result.User!)
            });
        }
        catch (Exception ex)
        {
            return BadRequest($"Login failed: {ex.Message}");
        }
    }

    [HttpPost("validate-token")]
    public IActionResult ValidateToken([FromBody] TokenValidationRequest request)
    {
        var principal = _jwtService.ValidateToken(request.Token);
        if (principal == null)
            return Unauthorized();

        var userId = principal.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        var username = principal.FindFirst(ClaimTypes.Name)?.Value;
        var email = principal.FindFirst(ClaimTypes.Email)?.Value;
        var displayName = principal.FindFirst("display_name")?.Value;
        var role = principal.FindFirst(ClaimTypes.Role)?.Value;
        var isActive = bool.Parse(principal.FindFirst("is_active")?.Value ?? "false");

        return Ok(new UserInfo
        {
            Id = Guid.Parse(userId!),
            Username = username!,
            DisplayName = displayName!,
            Email = email!,
            Role = role!,
            IsActive = isActive
        });
    }

    private async Task<DiscordTokenResponse?> ExchangeCodeForToken(string code)
    {
        var clientId = _configuration["Discord:ClientId"];
        var clientSecret = _configuration["Discord:ClientSecret"];
        var redirectUri = _configuration["Discord:RedirectUri"] ?? "http://localhost:7082/auth/discord-callback";

        var parameters = new Dictionary<string, string>
        {
            ["client_id"] = clientId!,
            ["client_secret"] = clientSecret!,
            ["grant_type"] = "authorization_code",
            ["code"] = code,
            ["redirect_uri"] = redirectUri
        };

        var content = new FormUrlEncodedContent(parameters);
        var response = await _httpClient.PostAsync("https://discord.com/api/oauth2/token", content);
        
        if (!response.IsSuccessStatusCode)
            return null;

        var responseContent = await response.Content.ReadAsStringAsync();
        return JsonSerializer.Deserialize<DiscordTokenResponse>(responseContent, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower
        });
    }

    private async Task<DiscordUser?> GetDiscordUserInfo(string accessToken)
    {
        _httpClient.DefaultRequestHeaders.Clear();
        _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {accessToken}");

        var response = await _httpClient.GetAsync("https://discord.com/api/users/@me");
        if (!response.IsSuccessStatusCode)
            return null;

        var responseContent = await response.Content.ReadAsStringAsync();
        return JsonSerializer.Deserialize<DiscordUser>(responseContent, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower
        });
    }

    private static UserInfo MapToUserInfo(User user)
    {
        return new UserInfo
        {
            Id = user.Id,
            Username = user.Username,
            DisplayName = user.DisplayName,
            Email = user.Email,
            Role = user.Role.ToString(),
            IsActive = user.IsActive
        };
    }
}

public class RegisterRequest
{
    [Required]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;
    
    [Required]
    [MinLength(3)]
    public string Username { get; set; } = string.Empty;
    
    [Required]
    [MinLength(6)]
    public string Password { get; set; } = string.Empty;
    
    [Required]
    public string DisplayName { get; set; } = string.Empty;
}

public class LoginRequest
{
    [Required]
    public string EmailOrUsername { get; set; } = string.Empty;
    
    [Required]
    public string Password { get; set; } = string.Empty;
}

public class DiscordCallbackRequest
{
    public string Code { get; set; } = string.Empty;
}

public class TokenValidationRequest
{
    public string Token { get; set; } = string.Empty;
}

public class AuthResponse
{
    public string Token { get; set; } = string.Empty;
    public UserInfo User { get; set; } = new();
}

public class UserInfo
{
    public Guid Id { get; set; }
    public string Username { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
    public bool IsActive { get; set; }
}

public class DiscordTokenResponse
{
    public string AccessToken { get; set; } = string.Empty;
    public string TokenType { get; set; } = string.Empty;
    public int ExpiresIn { get; set; }
    public string RefreshToken { get; set; } = string.Empty;
    public string Scope { get; set; } = string.Empty;
}

public class DiscordUser
{
    public string Id { get; set; } = string.Empty;
    public string Username { get; set; } = string.Empty;
    public string? GlobalName { get; set; }
    public string? Email { get; set; }
    public bool Verified { get; set; }
}
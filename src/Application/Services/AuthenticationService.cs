using PathfinderCampaignManager.Application.Abstractions;
using PathfinderCampaignManager.Domain.Entities;

namespace PathfinderCampaignManager.Application.Services;

public class AuthenticationService
{
    private readonly IUserRepository _userRepository;
    private readonly IPasswordService _passwordService;
    private readonly IJwtAuthenticationService _jwtService;

    public AuthenticationService(
        IUserRepository userRepository, 
        IPasswordService passwordService,
        IJwtAuthenticationService jwtService)
    {
        _userRepository = userRepository;
        _passwordService = passwordService;
        _jwtService = jwtService;
    }

    public async Task<AuthResult> RegisterAsync(string email, string username, string password, string displayName, CancellationToken cancellationToken = default)
    {
        // Check if user already exists
        if (await _userRepository.ExistsAsync(email, cancellationToken))
        {
            return AuthResult.Failure("An account with this email already exists.");
        }

        if (await _userRepository.UsernameExistsAsync(username, cancellationToken))
        {
            return AuthResult.Failure("This username is already taken.");
        }

        // Validate password (basic validation - you can extend this)
        if (string.IsNullOrWhiteSpace(password) || password.Length < 6)
        {
            return AuthResult.Failure("Password must be at least 6 characters long.");
        }

        // Hash the password
        var passwordHash = _passwordService.HashPassword(password);

        // Create the user
        var user = User.CreateWithPassword(email, username, displayName, passwordHash);

        // Save to database
        await _userRepository.AddAsync(user, cancellationToken);
        await _userRepository.SaveChangesAsync(cancellationToken);

        // Generate JWT token
        var token = _jwtService.GenerateToken(user);

        return AuthResult.Success(token, user);
    }

    public async Task<AuthResult> LoginAsync(string emailOrUsername, string password, CancellationToken cancellationToken = default)
    {
        // Find user by email or username
        var user = await _userRepository.GetByEmailAsync(emailOrUsername, cancellationToken);
        if (user == null)
        {
            user = await _userRepository.GetByUsernameAsync(emailOrUsername, cancellationToken);
        }

        if (user == null || !user.IsActive)
        {
            return AuthResult.Failure("Invalid email/username or password.");
        }

        // Check if user has a password (might be Discord-only user)
        if (!user.HasPassword())
        {
            return AuthResult.Failure("This account uses Discord authentication. Please sign in with Discord.");
        }

        // Verify password
        if (!_passwordService.VerifyPassword(password, user.PasswordHash!))
        {
            return AuthResult.Failure("Invalid email/username or password.");
        }

        // Update last login
        user.RecordLogin();
        _userRepository.Update(user);
        await _userRepository.SaveChangesAsync(cancellationToken);

        // Generate JWT token
        var token = _jwtService.GenerateToken(user);

        return AuthResult.Success(token, user);
    }

    public async Task<AuthResult> LoginWithDiscordAsync(string discordId, string email, string username, string displayName, CancellationToken cancellationToken = default)
    {
        // Try to find existing user by Discord ID
        var user = await _userRepository.GetByDiscordIdAsync(discordId, cancellationToken);

        if (user == null)
        {
            // Try to find by email (user might have registered with email first)
            user = await _userRepository.GetByEmailAsync(email, cancellationToken);
            
            if (user != null && string.IsNullOrEmpty(user.DiscordId))
            {
                // Link Discord to existing account
                // TODO: Add method to update Discord ID
                // For now, create new user to avoid conflicts
                user = null;
            }
        }

        if (user == null)
        {
            // Create new user with Discord
            user = User.CreateWithDiscord(email, username, displayName, discordId);
            await _userRepository.AddAsync(user, cancellationToken);
        }
        else
        {
            // Update login time
            user.RecordLogin();
            _userRepository.Update(user);
        }

        await _userRepository.SaveChangesAsync(cancellationToken);

        // Generate JWT token
        var token = _jwtService.GenerateToken(user);

        return AuthResult.Success(token, user);
    }
}

public class AuthResult
{
    public bool IsSuccess { get; private set; }
    public string? Token { get; private set; }
    public User? User { get; private set; }
    public string? ErrorMessage { get; private set; }

    private AuthResult(bool isSuccess, string? token, User? user, string? errorMessage)
    {
        IsSuccess = isSuccess;
        Token = token;
        User = user;
        ErrorMessage = errorMessage;
    }

    public static AuthResult Success(string token, User user)
        => new(true, token, user, null);

    public static AuthResult Failure(string errorMessage)
        => new(false, null, null, errorMessage);
}